using BookmarkLocker.Models;

namespace BookmarkLocker.Services
{
    public interface IBookmarkService
    {

        Task<IReadOnlyList<Bookmark>> GetBookmarksAsync(string userId, CancellationToken ct = default);
        Task<Bookmark?> GetBookmarkByIdAsync(string userId, string bookmarkId, CancellationToken ct = default);
        Task<Bookmark> CreateBookmarkAsync(Bookmark bookmark, CancellationToken ct = default);
        Task<Bookmark> UpdateBookmarkAsync(Bookmark bookmark, string? ifMatchEtag = null, CancellationToken ct = default);
        Task<bool> DeleteBookmarkAsync(string userId, string bookmarkId, CancellationToken ct = default);
    }
}
