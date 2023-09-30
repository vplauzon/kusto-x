using Kusto.Language;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.KustoState
{
    internal class StreamingBuffer<T>
    {
        #region Inner Types
        private record QueueItem(
            byte[] buffer,
            TaskCompletionSource completionSource);
        #endregion

        private readonly StreamingBufferOptions _options;
        private readonly ConcurrentQueue<QueueItem> _queue = new ConcurrentQueue<QueueItem>();

        public StreamingBuffer(StreamingBufferOptions? options = null)
        {
            _options = options == null
                ? new StreamingBufferOptions()
                : new StreamingBufferOptions(options);
        }

        public Task AppendRecords(
            IEnumerable<T> records,
            CancellationToken ct)
        {
            QueueRecords(records);

            throw new NotImplementedException();
        }

        private void QueueRecords(IEnumerable<T> records)
        {
            throw new NotImplementedException();
        }
    }
}