using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("email_blocks")]
public sealed class EmailBlockRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("template_id")]
    public Guid TemplateId { get; set; }

    [Column("block_order")]
    public int BlockOrder { get; set; }

    [Column("block_type")]
    public string BlockType { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;
}
