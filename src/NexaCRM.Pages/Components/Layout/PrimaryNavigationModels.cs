using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace NexaCRM.Pages.Components.Layout;

public sealed record PrimaryNavConfig(string Label, IReadOnlyList<SecondaryNavLink> Links);

public sealed record SecondaryNavLink(string Title, string Href, NavLinkMatch Match);

public enum PrimaryNavSection
{
    Dashboard,
    Deals,
    Leads,
    Settings
}
