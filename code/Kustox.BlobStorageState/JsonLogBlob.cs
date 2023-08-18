using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using System.Collections.Immutable;
using System.Text.Json;
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

        public async Task<IImmutableList<T>> ReadAllAsync(CancellationToken ct)
        {
            using (var stream = await _logBlob.DownloadContentAsync(ct))
            {
                var list = ImmutableArray.CreateBuilder<T>();

                while (true)
                {
                    var item = JsonSerializer.Deserialize<T>(stream, JSON_SERIALIZER_OPTIONS);

                    if (item != null)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        break;
                    }
                }

                return list.ToImmutable();
            }
        }
    }
}