using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.CustomerCenter;

namespace NexaCRM.WebClient.Services.Admin;

public sealed class CustomerCenterService : ICustomerCenterService
{
    private readonly List<CustomerCenterSummary> _summaries =
    [
        new()
        {
            TotalTickets = 42,
            OpenTickets = 5,
            AvgResponseMinutes = 18,
            SatisfactionScore = 4.6m,
            TopCategories = new[] { "계정", "결제", "기술" }
        }
    ];

    public Task<CustomerCenterSummary> GetSummaryAsync() =>
        Task.FromResult(_summaries.First());

    public Task<IReadOnlyList<CustomerFeedback>> GetFeedbackAsync() =>
        Task.FromResult<IReadOnlyList<CustomerFeedback>>(new List<CustomerFeedback>());
}
