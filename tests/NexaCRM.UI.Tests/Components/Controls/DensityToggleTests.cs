using Bunit;
using Microsoft.AspNetCore.Components;
using NexaCRM.UI.Components.Controls;
using Xunit;

namespace NexaCRM.UI.Tests.Components.Controls;

public class DensityToggleTests : TestContext
{
    [Fact]
    public void DensityToggle_DefaultsToComfortable()
    {
        // Arrange & Act
        var cut = RenderComponent<DensityToggle>();

        // Assert
        var buttons = cut.FindAll("button.nc-density-toggle__option");
        Assert.Equal("true", buttons[0].GetAttribute("aria-pressed"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void DensityToggle_ClickCompactEmitsValueChanged()
    {
        // Arrange
        DensityMode? received = null;

        var cut = RenderComponent<DensityToggle>(parameters => parameters
            .Add(p => p.Value, DensityMode.Comfortable)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<DensityMode>(this, mode => received = mode)));

        // Act
        cut.FindAll("button.nc-density-toggle__option")[1].Click();

        // Assert
        Assert.Equal(DensityMode.Compact, received);
    }
}
