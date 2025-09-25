namespace BookmarkLocker.Config
{
    public sealed class CosmosOptions
    {
        public string AccountEndpoint { get; set; }
        public string AccountKey { get; set; } = string.Empty;
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public string PartitionKeyPath { get; set; } = "/userId";

    }
}
