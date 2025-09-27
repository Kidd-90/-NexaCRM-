using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("deal_stages")]
public sealed class DealStageRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("sort_order")]
    public int SortOrder { get; set; }
}
