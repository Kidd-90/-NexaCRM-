using System;
using System.ComponentModel.DataAnnotations;

namespace NexaCRM.WebClient.Models.Db
{
    public class DbCustomer
    {
        [Display(Name = "ID")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "고객명은 필수입니다.")]
        [Display(Name = "고객명")]
        public string CustomerName { get; set; }

        [Display(Name = "연락처")]
        public string ContactNumber { get; set; }

        [Display(Name = "담당자")]
        public string AssignedTo { get; set; }

        [Display(Name = "분배일")]
        public DateTime AssignedDate { get; set; }

        [Display(Name = "DB 상태")]
        public DbStatus Status { get; set; }

        [Display(Name = "최종 컨택일")]
        public DateTime LastContactDate { get; set; }

        [Display(Name = "관심(중요) 표시")]
        public bool IsStarred { get; set; }

        [Display(Name = "전달자")]
        public string Assigner { get; set; }
    }
}
