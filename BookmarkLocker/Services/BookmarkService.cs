using BookmarkLocker.Models;

namespace BookmarkLocker.Services
{
    public class BookmarkService : IBookmarkService
    {
        public Task<Bookmark> CreateBookmarkAsync(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteBookmarkAsync(string userId, string bookmarkId)
        {
            throw new NotImplementedException();
        }

        public Task<Bookmark> GetBookmarkByIdAsync(string userId, string bookmarkId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Bookmark>> GetBookmarksAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Bookmark> UpdateBookmarkAsync(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }
    }
}
