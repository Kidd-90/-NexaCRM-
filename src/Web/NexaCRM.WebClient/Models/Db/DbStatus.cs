using System.ComponentModel.DataAnnotations;

namespace NexaCRM.WebClient.Models.Db
{
    public enum DbStatus
    {
        [Display(Name = "신규")]
        New,

        [Display(Name = "상담중")]
        InProgress,

        [Display(Name = "부재중")]
        NoAnswer,

        [Display(Name = "계약완료")]
        Completed,

        [Display(Name = "보류")]
        OnHold
    }
}
