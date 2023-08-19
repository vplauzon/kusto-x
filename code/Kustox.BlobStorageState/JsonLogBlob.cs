using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kustox.BlobStorageState
{
    internal class JsonLogBlob<T>
    {
        private readonly static JsonSerializerOptions JSON_SERIALIZER_OPTIONS =
            new JsonSerializerOptions
            {
                WriteIndented = false
            };
        private readonly LogBlob _logBlob;
        private readonly Func<IEnumerable<T>, IImmutableList<T>>? _compactor;

        static JsonLogBlob()
        {
            JSON_SERIALIZER_OPTIONS.Converters.Add(new JsonStringEnumConverter());
        }

        public JsonLogBlob(
            DataLakeDirectoryClient folder,
            BlobContainerClient containerClient,
            string blobName,
            Func<IEnumerable<T>, IImmutableList<T>>? compactor = null)
        {
            _logBlob = new LogBlob(folder, containerClient, blobName);
            _compactor = compactor;
        }

        public async Task<IImmutableList<T>> ReadAllAsync(CancellationToken ct)
        {
            using (var stream = await _logBlob.DownloadContentAsync(ct))
            using (var reader = new StreamReader(stream))
            {
                var allText = await reader.ReadToEndAsync();
                var lines = allText
                    .Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line));
                var items = lines
                    .Select(line => JsonSerializer.Deserialize<T>(line, JSON_SERIALIZER_OPTIONS))
                    .ToImmutableArray();
                var compactedItems = _compactor != null
                    ? _compactor(items)
                    : items;

                return compactedItems;
            }
        }

        public async Task AppendAsync(IEnumerable<T> data, CancellationToken ct)
        {
            using (var stream = new MemoryStream())
            {
                foreach (var item in data)
                {
                    JsonSerializer.Serialize(stream, item, JSON_SERIALIZER_OPTIONS);
                    //  Append "return"
                    stream.WriteByte((byte)'\n');
                }
                stream.Position = 0;
                await _logBlob.AppendAsync(stream, ct);
            }
        }
    }
}