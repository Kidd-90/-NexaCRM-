using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IMarketingCampaignService
    {
        Task<IEnumerable<MarketingCampaign>> GetCampaignsAsync();
        Task<MarketingCampaign> GetCampaignByIdAsync(int id);
        Task CreateCampaignAsync(MarketingCampaign campaign);
        Task UpdateCampaignAsync(MarketingCampaign campaign);
        Task DeleteCampaignAsync(int id);
    }
}
