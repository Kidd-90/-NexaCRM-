using System;
using System.ComponentModel.DataAnnotations;

namespace NexaCRM.Services.Admin.Models.Db
{
    public class DbCustomer
    {
        [Display(Name = "ID")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // This is the foreign key to the Contact model
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Contact")]
        public string? ContactNumber { get; set; }

        [Display(Name = "Group")]
        public string? Group { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; }

        [Display(Name = "Status")]
        public DbStatus Status { get; set; }

        [Display(Name = "Last Contact Date")]
        public DateTime LastContactDate { get; set; }

        [Display(Name = "Starred")]
        public bool IsStarred { get; set; }

        [Display(Name = "Assigned By")]
        public string? Assigner { get; set; }

        // Optional archive flag for advanced management workflows
        public bool IsArchived { get; set; }

        // Optional extended fields for dedupe rules (all nullable)
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? JobTitle { get; set; }
        public string? MaritalStatus { get; set; }
        public string? ProofNumber { get; set; }
        public decimal? DbPrice { get; set; }
        public string? Headquarters { get; set; }
        public string? InsuranceName { get; set; }
        public DateTime? CarJoinDate { get; set; }
        public string? Notes { get; set; }
    }
}
