using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Kusto.Data;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class LogBlob
    {
        private readonly DataLakeDirectoryClient _folder;
        private readonly string _blobName;
        private readonly AppendBlobClient _blob;

        public LogBlob(
            DataLakeDirectoryClient folder,
            BlobContainerClient containerClient,
            string blobName)
        {
            _folder = folder;
            _blobName = blobName;
            _blob = containerClient.GetAppendBlobClient($"{_folder.Path}/{blobName}");
        }

        public async Task CreateIfNotExistsAsync(CancellationToken ct)
        {
            await _blob.CreateIfNotExistsAsync(null, ct);
        }

        public async Task AppendAsync(Stream stream, CancellationToken ct)
        {
            await _blob.AppendBlockAsync(stream, null, ct);
        }

        public async Task<Stream> DownloadContentAsync(CancellationToken ct)
        {
            var result = await _blob.DownloadContentAsync(ct);
            var stream = result.Value.Content.ToStream();

            return stream;
        }
    }
}