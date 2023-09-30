using Kusto.Data.Common;
using Kusto.Ingest;
using Kusto.Ingest.Exceptions;
using Kusto.Language;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.KustoState
{
    internal class StreamingBuffer
    {
        #region Inner Types
        private record QueueItem(
            byte[] buffer,
            TaskCompletionSource completionSource);
        #endregion

        private const long MAX_PAYLOAD_SIZE = 4 * 1024 * 1024;
        private readonly IKustoIngestClient _ingestClient;
        private readonly string _dbName;
        private readonly string _tableName;
        private readonly StreamingBufferOptions _options;
        private readonly ConcurrentQueue<QueueItem> _queue = new ConcurrentQueue<QueueItem>();
        private Task _streamingWorkerTask = Task.CompletedTask;

        public StreamingBuffer(
            IKustoIngestClient ingestClient,
            string dbName,
            string tableName,
            StreamingBufferOptions? options = null)
        {
            _ingestClient = ingestClient;
            _dbName = dbName;
            _tableName = tableName;
            _options = options == null
                ? new StreamingBufferOptions()
                : new StreamingBufferOptions(options);
        }

        public async Task AppendRecords(
            IEnumerable<object> records,
            CancellationToken ct)
        {
            var recordTasks = QueueRecords(records);
            var recordsAllStreamed = Task.WhenAll(recordTasks);

            //  Either all records get streamed or batch delay expires
            //  (and streaming worker available)
            await Task.WhenAny(
                recordsAllStreamed,
                Task.WhenAll(Task.Delay(_options.BatchInterval, ct), _streamingWorkerTask));

            while (true)
            {
                if (recordsAllStreamed.IsCompleted)
                {   //  Records got streamed by another thread
                    return;
                }

                var currentStreamingWorkerTask = _streamingWorkerTask;

                if (currentStreamingWorkerTask.IsCompleted)
                {
                    var newStreamingWorkerSource = new TaskCompletionSource();
                    var presentStreamingWorkerTask = Interlocked.CompareExchange(
                        ref _streamingWorkerTask,
                        newStreamingWorkerSource.Task,
                        currentStreamingWorkerTask);

                    if (object.ReferenceEquals(
                        presentStreamingWorkerTask,
                        currentStreamingWorkerTask))
                    {   //  This thread won the worker election
                        await StreamInBatchAsync(recordsAllStreamed, ct);
                    }
                }
                await Task.WhenAny(
                    recordsAllStreamed,
                    Task.Delay(_options.ElectionRetryInterval, ct));
            }
        }

        private async Task StreamInBatchAsync(Task recordsAllStreamed, CancellationToken ct)
        {
            var itemsToStream = new List<QueueItem>();
            var payloadSize = 0;

            try
            {
                while (!recordsAllStreamed.IsCompleted)
                {
                    if (ct.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Couldn't stream");
                    }
                    if (_queue.TryPeek(out var queueItem))
                    {
                        if (queueItem.buffer.Length + payloadSize < MAX_PAYLOAD_SIZE)
                        {
                            if (_queue.TryDequeue(out var dequeuedItem))
                            {
                                if (!object.ReferenceEquals(queueItem, dequeuedItem))
                                {
                                    throw new InvalidOperationException(
                                        "Queue items should be the same as no one can dequeue");
                                }
                                itemsToStream.Add(dequeuedItem);
                                payloadSize += dequeuedItem.buffer.Length;
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    "Queue shouldn't be empty as no one can dequeue");
                            }
                        }
                        else
                        {   //  Payload is as big as can be
                            if (!itemsToStream.Any())
                            {
                                throw new InvalidOperationException(
                                    "The original records can't be found?");
                            }
                            if (await StreamBatchAsync(itemsToStream, ct))
                            {
                                payloadSize = 0;
                                itemsToStream.Clear();
                            }
                            else
                            {
                                await Task.WhenAny(
                                    Task.Delay(_options.ThrottleRetryInterval, ct),
                                    recordsAllStreamed);
                            }
                        }
                    }
                }
            }
            finally
            {   //  Re-queue items that couldn't be streamed
                foreach (var item in itemsToStream)
                {
                    _queue.Enqueue(item);
                }
            }
        }

        private async Task<bool> StreamBatchAsync(
            IEnumerable<QueueItem> itemsToStream,
            CancellationToken ct)
        {
            using (var stream = new MemoryStream(itemsToStream.Sum(i => i.buffer.Length)))
            {
                foreach (var item in itemsToStream)
                {
                    stream.Write(item.buffer);
                }
                stream.Position = 0;
                try
                {
                    await _ingestClient.IngestFromStreamAsync(
                        stream,
                        new KustoIngestionProperties(_dbName, _tableName)
                        {
                            Format = DataSourceFormat.json
                        });

                    return true;
                }
                catch (StreamingIngestClientException)
                {
                    return false;
                }
            }
        }

        private Task StreamBatchAsync(
            List<QueueItem> itemsToStream,
            Task recordsAllStreamed,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Task> QueueRecords(IEnumerable<object> records)
        {
            var builder = ImmutableArray<Task>.Empty.ToBuilder();

            foreach (var record in records)
            {
                var buffer = KustoHelper.Serialize(record);
                var completionSource = new TaskCompletionSource();

                builder.Add(completionSource.Task);
                _queue.Enqueue(new QueueItem(buffer, completionSource));
            }

            return builder.ToImmutable();
        }
    }
}