using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockMarketingCampaignService : IMarketingCampaignService
    {
        private readonly List<MarketingCampaign> _campaigns;

        public MockMarketingCampaignService()
        {
            _campaigns = new List<MarketingCampaign>
            {
                new MarketingCampaign { Id = 1, Name = "Summer Sale", Type = "Email", Status = CampaignStatus.Active, StartDate = new DateTime(2023, 7, 1), EndDate = new DateTime(2023, 7, 31), Budget = 5000, ROI = 0.15m },
                new MarketingCampaign { Id = 2, Name = "Product Launch", Type = "Social Media", Status = CampaignStatus.Scheduled, StartDate = new DateTime(2023, 8, 15), EndDate = new DateTime(2023, 9, 15), Budget = 10000, ROI = 0.22m },
                new MarketingCampaign { Id = 3, Name = "Holiday Promotion", Type = "Ads", Status = CampaignStatus.Completed, StartDate = new DateTime(2023, 12, 1), EndDate = new DateTime(2023, 12, 31), Budget = 8000, ROI = 0.28m },
                new MarketingCampaign { Id = 4, Name = "Spring Collection", Type = "Email", Status = CampaignStatus.Active, StartDate = new DateTime(2024, 3, 1), EndDate = new DateTime(2024, 3, 31), Budget = 6000, ROI = 0.18m },
                new MarketingCampaign { Id = 5, Name = "Anniversary Event", Type = "Social Media", Status = CampaignStatus.Draft, StartDate = new DateTime(2024, 5, 1), EndDate = new DateTime(2024, 5, 31), Budget = 12000, ROI = 0.25m }
            };
        }

        public Task<IEnumerable<MarketingCampaign>> GetCampaignsAsync()
        {
            return Task.FromResult<IEnumerable<MarketingCampaign>>(_campaigns);
        }

        public Task<MarketingCampaign> GetCampaignByIdAsync(int id)
        {
            var campaign = _campaigns.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(campaign);
        }

        public Task CreateCampaignAsync(MarketingCampaign campaign)
        {
            campaign.Id = _campaigns.Max(c => c.Id) + 1;
            _campaigns.Add(campaign);
            return Task.CompletedTask;
        }

        public Task UpdateCampaignAsync(MarketingCampaign campaign)
        {
            var existingCampaign = _campaigns.FirstOrDefault(c => c.Id == campaign.Id);
            if (existingCampaign != null)
            {
                existingCampaign.Name = campaign.Name;
                existingCampaign.Type = campaign.Type;
                existingCampaign.Status = campaign.Status;
                existingCampaign.StartDate = campaign.StartDate;
                existingCampaign.EndDate = campaign.EndDate;
                existingCampaign.Budget = campaign.Budget;
                existingCampaign.ROI = campaign.ROI;
            }
            return Task.CompletedTask;
        }

        public Task DeleteCampaignAsync(int id)
        {
            var campaign = _campaigns.FirstOrDefault(c => c.Id == id);
            if (campaign != null)
            {
                _campaigns.Remove(campaign);
            }
            return Task.CompletedTask;
        }
    }
}
