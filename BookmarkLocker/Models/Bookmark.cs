namespace BookmarkLocker.Models
{
    public record Bookmark
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
