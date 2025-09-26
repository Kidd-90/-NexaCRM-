using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("sales_appointments")]
public sealed class SalesAppointmentRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("start_datetime")]
    public DateTime StartDateTime { get; set; }

    [Column("end_datetime")]
    public DateTime EndDateTime { get; set; }

    [Column("contact_id")]
    public int ContactId { get; set; }

    [Column("contact_name")]
    public string? ContactName { get; set; }

    [Column("contact_company")]
    public string? ContactCompany { get; set; }

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
