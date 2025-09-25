using BookmarkLocker.Config;
using BookmarkLocker.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;

namespace BookmarkLocker.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ILogger<BookmarkService> _logger;
        private readonly Container _container;

        public BookmarkService(
            ILogger<BookmarkService> logger,
            CosmosClient cosmosClient,
            IOptions<CosmosOptions> options)
        {
            _logger = logger;
            var o = options.Value;

            var database = cosmosClient.GetDatabase(o.DatabaseName);
            _container = database.GetContainer(o.ContainerName);
        }

        public async Task<Bookmark> CreateBookmarkAsync(Bookmark bookmark, CancellationToken ct = default)
        {
            Validate(bookmark);

            try
            {
                var result = await _container.CreateItemAsync(bookmark, new PartitionKey(bookmark.UserId), cancellationToken: ct);
                Log("CreateItem", result.RequestCharge, result.StatusCode, result.ETag, result.Diagnostics);
                return result.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogWarning(ex, "Create conflict for {Id}/{UserId}", bookmark.Id, bookmark.UserId);
                throw;
            }
        }

        public async Task<bool> DeleteBookmarkAsync(string userId, string bookmarkId, CancellationToken ct = default)
        {
            try
            {
                var resp = await _container.DeleteItemAsync<Bookmark>(bookmarkId, new PartitionKey(userId), cancellationToken: ct);
                Log("DeleteItem", resp.RequestCharge, resp.StatusCode, resp.ETag, resp.Diagnostics);
                return resp.StatusCode == HttpStatusCode.NoContent;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Already gone: {Id}/{UserId}", bookmarkId, userId);
                return false;
            }
        }

        public async Task<Bookmark?> GetBookmarkByIdAsync(string userId, string bookmarkId, CancellationToken ct = default)
        {
            try
            {
                var resp = await _container.ReadItemAsync<Bookmark>(bookmarkId, new PartitionKey(userId), cancellationToken: ct);
                Log("ReadItem", resp.RequestCharge, resp.StatusCode, resp.ETag, resp.Diagnostics);
                return resp.Resource with { ETag = resp.ETag };
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Not found: {Id}/{UserId}", bookmarkId, userId);
                return null;
            }
        }

        public async Task<IReadOnlyList<Bookmark>> GetBookmarksAsync(string userId, CancellationToken ct = default)
        {
            var q = new QueryDefinition("SELECT c.id, c.userId, c.title, c.url, c.tags, c.notes, c.createdAt FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", userId);

            var opts = new QueryRequestOptions { PartitionKey = new PartitionKey(userId), MaxItemCount = 100 };
            var it = _container.GetItemQueryIterator<Bookmark>(q, requestOptions: opts);

            var items = new List<Bookmark>();
            double totalRu = 0;

            while (it.HasMoreResults)
            {
                var page = await it.ReadNextAsync(ct);
                totalRu += page.RequestCharge;
                items.AddRange(page.Select(p => p with { ETag = page.ETag })); 
            }

            _logger.LogInformation("Query GetAll: {Count} items, {RU:F2} RU", items.Count, totalRu);
            return items;
        }

        public async Task<Bookmark> UpdateBookmarkAsync(Bookmark bookmark, string? ifMatchEtag = null, CancellationToken ct = default)
        {
            Validate(bookmark);

            var req = new ItemRequestOptions();
            if (!string.IsNullOrWhiteSpace(ifMatchEtag))
                req.IfMatchEtag = ifMatchEtag;

            try
            {
                var resp = await _container.UpsertItemAsync(bookmark, new PartitionKey(bookmark.UserId), req, ct);
                Log("UpsertItem", resp.RequestCharge, resp.StatusCode, resp.ETag, resp.Diagnostics);
                return resp.Resource with { ETag = resp.ETag };
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                _logger.LogWarning("ETag mismatch on update for {Id}/{UserId}", bookmark.Id, bookmark.UserId);
                return null;
            }
        }

        private static void Validate(Bookmark b)
        {
            if (string.IsNullOrWhiteSpace(b.Id)) throw new ArgumentException("Id is required");
            if (string.IsNullOrWhiteSpace(b.UserId)) throw new ArgumentException("UserId (partition key) is required");
        }

        private void Log(string op, double ru, HttpStatusCode status, string? etag, CosmosDiagnostics? diag)
        {
            _logger.LogInformation("{Op}: {RU:F2} RU, {Status}, etag={Etag}", op, ru, status, etag);

            _logger.LogDebug("Cosmos diagnostics for {Op}:\n{Diag}", op, diag?.ToString());
        }
    }
}
