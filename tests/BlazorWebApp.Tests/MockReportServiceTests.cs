using System.Collections.Generic;
using System.Linq;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Mock;

namespace BlazorWebApp.Tests;

public class MockReportServiceTests
{
    [Fact]
    public async System.Threading.Tasks.Task SaveReportDefinitionAsync_PersistsDefinition()
    {
        var service = new MockReportService();
        var def = new ReportDefinition { Name = "Test", SelectedFields = new List<string> { "A" } };
        await service.SaveReportDefinitionAsync(def);
        var defs = await service.GetReportDefinitionsAsync();
        Assert.Contains(defs, d => d.Name == "Test");
    }

    [Fact]
    public async System.Threading.Tasks.Task GenerateReportAsync_ReturnsDataForFields()
    {
        var service = new MockReportService();
        var def = new ReportDefinition { Name = "Test", SelectedFields = new List<string> { "Field1", "Field2" } };
        var data = await service.GenerateReportAsync(def);
        Assert.Equal(def.SelectedFields.Count, data.Data!.Count);
        foreach (var field in def.SelectedFields)
        {
            Assert.True(data.Data.ContainsKey(field));
        }
    }
}
