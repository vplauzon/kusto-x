using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using System.Collections.Immutable;
using System.Text.Json;

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

        public JsonLogBlob(
            DataLakeDirectoryClient folder,
            BlobContainerClient containerClient,
            string blobName)
        {
            _logBlob = new LogBlob(folder, containerClient, blobName);
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