using NexaCRM.WebClient.Pages;
using Xunit;
using System.Reflection;

namespace NexaCRM.WebClient.UnitTests.Pages;

public class AllDbListPageTests
{
    [Fact]
    public void AllDbListPage_Has_Filter_Functionality()
    {
        var pageType = typeof(AllDbListPage);
        var methods = pageType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Contains(methods, m => m.Name.Contains("FilterCustomers"));
        Assert.Contains(methods, m => m.Name.Contains("OnFilterInput"));
    }

    [Fact]
    public void ContactDetailPage_Has_RelatedDeals_Field()
    {
        var pageType = typeof(ContactDetailPage);
        var field = pageType.GetField("relatedDeals", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
    }
}
