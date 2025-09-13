using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Services.Mock;

namespace BlazorWebApp.Tests;

public class MockDbDataServiceTests
{
    [Fact]
    public async Task AssignDbToAgentAsync_AssignsCustomer()
    {
        var service = new MockDbDataService();
        var unassigned = await service.GetUnassignedDbListAsync();
        var customer = unassigned.First();
        await service.AssignDbToAgentAsync(customer.ContactId, "이영업");
        var assigned = (await service.GetAllDbListAsync()).First(c => c.ContactId == customer.ContactId);
        Assert.Equal("이영업", assigned.AssignedTo);
    }

    [Fact]
    public async Task RecallDbAsync_RemovesAssignment()
    {
        var service = new MockDbDataService();
        var assignedCustomer = (await service.GetAllDbListAsync()).First(c => c.AssignedTo != null);
        await service.RecallDbAsync(assignedCustomer.ContactId);
        var recalled = (await service.GetAllDbListAsync()).First(c => c.ContactId == assignedCustomer.ContactId);
        Assert.Null(recalled.AssignedTo);
    }
}
