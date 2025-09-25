using System.ComponentModel.DataAnnotations;

namespace BookmarkLocker.DTOs
{
    public sealed record UpdateBookmarkRequest(
        [Required] string Id,
        [Required] string UserId,
        [Required] string Title,
        [Required, Url] string Url,
        IEnumerable<string> Tags,
        string Notes
    );
}
