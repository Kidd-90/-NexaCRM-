using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("profiles")]
public sealed class ProfileLookupRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("username")]
    public string? Username { get; set; }
}
