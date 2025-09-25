using BookmarkLocker.Models;
using System.ComponentModel;

namespace BookmarkLocker.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ILogger<BookmarkService> _logger;
        private readonly Container _container;

        public BookmarkService(ILogger<BookmarkService> logger, Container container)
        {
            _logger = logger;
            _container = container;
        }

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
