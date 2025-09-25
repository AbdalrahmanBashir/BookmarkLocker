using BookmarkLocker.DTOs;
using BookmarkLocker.Models;
using BookmarkLocker.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookmarkLocker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookmarkController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly ILogger<BookmarkController> _logger;
        public BookmarkController(IBookmarkService bookmarkService, ILogger<BookmarkController> logger)
        {
            _bookmarkService = bookmarkService;
            _logger = logger;
        }

        
        [HttpPost]
        [ProducesResponseType(typeof(Bookmark), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateBookmarkRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var bookmark = new Bookmark
            {
                Id = req.Id,
                UserId = req.UserId,
                Title = req.Title,
                Url = req.Url,
                Tags = req.Tags,
                Notes = req.Notes,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var created = await _bookmarkService.CreateBookmarkAsync(bookmark, ct);
                // Expose ETag to clients
                if (!string.IsNullOrWhiteSpace(created.ETag))
                    Response.Headers.ETag = created.ETag;

                // canonical “location” for the resource
                return CreatedAtAction(nameof(GetById), new { userId = created.UserId, id = created.Id }, created);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex) when ((int)ex.StatusCode == 409)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Bookmark already exists",
                    Detail = $"A bookmark with id '{req.Id}' for user '{req.UserId}' already exists.",
                    Status = StatusCodes.Status409Conflict
                });
            }
        }

        
        [HttpGet("{userId}/{id}")]
        [ProducesResponseType(typeof(Bookmark), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] string userId, [FromRoute] string id, CancellationToken ct)
        {
            var b = await _bookmarkService.GetBookmarkByIdAsync(userId, id, ct);
            if (b is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(b.ETag))
                Response.Headers.ETag = b.ETag;

            return Ok(b);
        }

        
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IReadOnlyList<Bookmark>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromRoute] string userId, CancellationToken ct)
        {
            var items = await _bookmarkService.GetBookmarksAsync(userId, ct);
            return Ok(items);
        }

        
        [HttpPut]
        [ProducesResponseType(typeof(Bookmark), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public async Task<IActionResult> Update([FromBody] UpdateBookmarkRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // If-Match header is optional; when present, enables optimistic concurrency
            var ifMatch = Request.Headers.IfMatch.ToString();
            if (string.Equals(ifMatch, "*", StringComparison.Ordinal)) ifMatch = null; // allow wildcard but treat as no-op

            var bookmark = new Bookmark
            {
                Id = req.Id,
                UserId = req.UserId,
                Title = req.Title,
                Url = req.Url,
                Tags = req.Tags,
                Notes = req.Notes,
                CreatedAt = DateTime.UtcNow
            };

            var updated = await _bookmarkService.UpdateBookmarkAsync(bookmark, ifMatch, ct);
            if (updated is null)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed, new ProblemDetails
                {
                    Title = "ETag precondition failed",
                    Detail = "The resource has been modified by someone else. Re-fetch and retry.",
                    Status = StatusCodes.Status412PreconditionFailed
                });
            }

            if (!string.IsNullOrWhiteSpace(updated.ETag))
                Response.Headers.ETag = updated.ETag;

            return Ok(updated);
        }


        
        [HttpDelete("{userId}/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] string userId, [FromRoute] string id, CancellationToken ct)
        {
            var removed = await _bookmarkService.DeleteBookmarkAsync(userId, id, ct);
            return removed ? NoContent() : NotFound();
        }
    }
}
