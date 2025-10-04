using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("db_customers")]
public sealed class DbCustomerRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("contact_id")]
    public int ContactId { get; set; }

    [Column("customer_name")]
    public string? CustomerName { get; set; }

    [Column("contact_number")]
    public string? ContactNumber { get; set; }

    [Column("\"group\"")]
    public string? Group { get; set; }

    [Column("assigned_to")]
    public string? AssignedTo { get; set; }

    [Column("assigner")]
    public string? Assigner { get; set; }

    [Column("assigned_date")]
    public DateTime? AssignedDate { get; set; }

    [Column("last_contact_date")]
    public DateTime? LastContactDate { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("is_starred")]
    public bool? IsStarred { get; set; }

    [Column("is_archived")]
    public bool? IsArchived { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("job_title")]
    public string? JobTitle { get; set; }

    [Column("marital_status")]
    public string? MaritalStatus { get; set; }

    [Column("proof_number")]
    public string? ProofNumber { get; set; }

    [Column("db_price")]
    public decimal? DbPrice { get; set; }

    [Column("headquarters")]
    public string? Headquarters { get; set; }

    [Column("insurance_name")]
    public string? InsuranceName { get; set; }

    [Column("car_join_date")]
    public DateTime? CarJoinDate { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
