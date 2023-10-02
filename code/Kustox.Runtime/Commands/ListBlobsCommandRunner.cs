using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.Runtime.Commands
{
    internal class ListBlobsCommandRunner : CommandRunnerBase
    {
        public ListBlobsCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            var rootUrl = new Uri(command.ListBlobsCommand!.RootUrl);
            var blobClient = new BlobClient(rootUrl, ConnectionProvider.Credential);
            var containerClient = blobClient.GetParentBlobContainerClient();
            var pageable = containerClient.GetBlobsAsync(
                prefix: blobClient.Name + "/",
                cancellationToken: ct);
            var blobs = await ListBlobsAsync(pageable);
            var blobRepresentation = blobs
                .Select(i => new
                {
                    Name = i.Name,
                    ContentLength = i.Properties.ContentLength,
                    ContentType = i.Properties.ContentType
                });
            var columns = new[]
            {
                new ColumnSpecification("Name", typeof(string)),
                new ColumnSpecification("Length", typeof(long)),
                new ColumnSpecification("ContentType", typeof(string))
            }.ToImmutableArray();
            var data = blobRepresentation
                .Select(b => new object?[]
                {
                    b.Name,
                    b.ContentLength,
                    b.ContentType
                }.ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();
            var result = new TableResult(false, columns, data);

            return result;
        }

        private static async Task<IImmutableList<BlobItem>> ListBlobsAsync(
            AsyncPageable<BlobItem> pageable)
        {
            var builder = ImmutableArray<BlobItem>.Empty.ToBuilder();

            await foreach (var item in pageable)
            {
                builder.Add(item);
            }

            return builder.ToImmutableArray();
        }
    }
}