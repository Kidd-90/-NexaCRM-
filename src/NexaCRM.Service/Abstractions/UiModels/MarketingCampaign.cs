using NexaCRM.UI.Models.Enums;
using System;

namespace NexaCRM.UI.Models
{
    public class MarketingCampaign
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public CampaignStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ROI { get; set; }
    }
}
