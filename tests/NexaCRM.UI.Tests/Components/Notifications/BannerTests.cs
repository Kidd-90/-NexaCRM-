using System;
using Bunit;
using Microsoft.AspNetCore.Components;
using NexaCRM.UI.Components.Notifications;
using Xunit;

namespace NexaCRM.UI.Tests.Components.Notifications;

public class BannerTests : TestContext
{
    [Fact]
    public void Banner_RendersTitleFragmentAndDescriptionText()
    {
        // Arrange & Act
        var cut = RenderComponent<Banner>(parameters => parameters
            .Add(p => p.Title, builder => builder.AddContent(0, "테스트 제목"))
            .Add(p => p.DescriptionText, "세부 메시지"));

        // Assert
        var title = cut.Find(".nc-banner__title");
        Assert.Equal("테스트 제목", title.TextContent.Trim());

        var description = cut.Find(".nc-banner__description");
        Assert.Equal("세부 메시지", description.TextContent.Trim());
    }

    [Fact]
    public void Banner_UsesTitleTextWhenFragmentMissing()
    {
        // Arrange & Act
        var cut = RenderComponent<Banner>(parameters => parameters
            .Add(p => p.TitleText, "System Notice"));

        // Assert
        var title = cut.Find(".nc-banner__title");
        Assert.Equal("System Notice", title.TextContent.Trim());
    }

    [Fact]
    public void Banner_WhenDismissibleInvokesCallback()
    {
        // Arrange
        var dismissed = false;

        var cut = RenderComponent<Banner>(parameters => parameters
            .Add(p => p.TitleText, "Closable Banner")
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, () => dismissed = true)));

        // Act
        cut.Find("button.nc-banner__dismiss").Click();

        // Assert
        Assert.True(dismissed);
    }

    [Fact]
    public void Banner_DefaultsToPoliteAriaLive()
    {
        // Arrange & Act
        var cut = RenderComponent<Banner>(parameters => parameters
            .Add(p => p.TitleText, "정보 배너"));

        // Assert
        var section = cut.Find("section.nc-banner");
        Assert.Equal("status", section.GetAttribute("role"));
        Assert.Equal("polite", section.GetAttribute("aria-live"));
        Assert.Equal("info", section.GetAttribute("data-variant"));
    }

    [Fact]
    public void Banner_WithAlertRoleSetsAssertiveAriaLive()
    {
        // Arrange & Act
        var cut = RenderComponent<Banner>(parameters => parameters
            .Add(p => p.TitleText, "경고 배너")
            .Add(p => p.Role, "alert"));

        // Assert
        var section = cut.Find("section.nc-banner");
        Assert.Equal("alert", section.GetAttribute("role"));
        Assert.Equal("assertive", section.GetAttribute("aria-live"));
    }

    [Fact]
    public void Banner_CustomAriaLiveOverridesDefault()
    {
        // Arrange & Act
        var cut = RenderComponent<Banner>(parameters => parameters
            .Add(p => p.TitleText, "사용자 정의")
            .Add(p => p.Role, "alert")
            .Add(p => p.AriaLive, "polite"));

        // Assert
        var section = cut.Find("section.nc-banner");
        Assert.Equal("polite", section.GetAttribute("aria-live"));
    }

    [Fact]
    public void Banner_ThrowsWhenTitleMissing()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => RenderComponent<Banner>());
        Assert.Equal("Banner requires either a Title fragment or TitleText value.", exception.Message);
    }
}
