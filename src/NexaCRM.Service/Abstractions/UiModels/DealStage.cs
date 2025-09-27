namespace NexaCRM.UI.Models
{
    public sealed class DealStage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
