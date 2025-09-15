using System.Threading.Tasks;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Mock;

namespace BlazorWebApp.Tests;

public class MockEmailTemplateServiceTests
{
    [Fact]
    public async System.Threading.Tasks.Task SaveAndLoadTemplateAsync_Works()
    {
        var service = new MockEmailTemplateService();
        var template = new EmailTemplate { Subject = "Test" };
        await service.SaveTemplateAsync(template);
        var loaded = await service.LoadTemplateAsync(template.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Test", loaded!.Subject);
    }
}
