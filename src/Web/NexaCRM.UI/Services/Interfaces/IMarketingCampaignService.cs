using NexaCRM.WebClient.Models;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IMarketingCampaignService
    {
        System.Threading.Tasks.Task<IEnumerable<MarketingCampaign>> GetCampaignsAsync();
        System.Threading.Tasks.Task<MarketingCampaign?> GetCampaignByIdAsync(int id);
        System.Threading.Tasks.Task CreateCampaignAsync(MarketingCampaign campaign);
        System.Threading.Tasks.Task UpdateCampaignAsync(MarketingCampaign campaign);
        System.Threading.Tasks.Task DeleteCampaignAsync(int id);
    }
}
