using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace BuildingBlocks.Common.Supabase.Data.Contacts;

/// <summary>
/// Represents the Supabase <c>contacts</c> table for PostgREST queries.
/// </summary>
[Table("contacts")]
public sealed class SupabaseContactRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
