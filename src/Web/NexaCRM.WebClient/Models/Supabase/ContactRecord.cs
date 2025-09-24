using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("contacts")]
public sealed class ContactRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

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
    public int? CompanyId { get; set; }

    [Column("company_name")]
    public string? CompanyName { get; set; }

    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }
}
