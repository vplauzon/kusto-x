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
        private readonly BlobContainerClient _containerClient;
        private readonly string _blobName;
        private readonly AppendBlobClient _blob;
        private bool _doesExist = false;

        public LogBlob(
            DataLakeDirectoryClient folder,
            BlobContainerClient containerClient,
            string blobName)
        {
            _folder = folder;
            _containerClient = containerClient;
            _blobName = blobName;
            _blob = containerClient.GetAppendBlobClient($"{_folder.Path}/{blobName}");
        }

        public async Task AppendAsync(Stream stream, CancellationToken ct)
        {
            if (!_doesExist)
            {
                _doesExist = true;
                await _blob.CreateIfNotExistsAsync(null, ct);
            }
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