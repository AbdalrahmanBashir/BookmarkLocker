using BookmarkLocker.Models;

namespace BookmarkLocker.Services
{
    public interface IBookmarkService
    {
        Task<IEnumerable<Bookmark>> GetBookmarksAsync(string userId);
        Task<Bookmark> GetBookmarkByIdAsync(string userId, string bookmarkId);
        Task<Bookmark> CreateBookmarkAsync(Bookmark bookmark);
        Task<Bookmark> UpdateBookmarkAsync(Bookmark bookmark);
        Task<bool> DeleteBookmarkAsync(string userId, string bookmarkId);
    }
}
