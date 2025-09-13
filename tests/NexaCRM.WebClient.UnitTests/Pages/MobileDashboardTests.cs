using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NexaCRM.WebClient.Pages;
using NexaCRM.WebClient.Components.UI;
using Xunit;
using Microsoft.AspNetCore.Components;
using Moq;
using AngleSharp.Dom;

namespace NexaCRM.WebClient.UnitTests.Pages
{
    public class MobileDashboardTests : TestContext
    {
        public MobileDashboardTests()
        {
            // Setup services
            Services.AddSingleton<NavigationManager>(new MockNavigationManager());
            Services.AddSingleton(Mock.Of<IJSRuntime>());
            Services.AddSingleton<IStringLocalizer<MainDashboard>>(new MockStringLocalizer());
            Services.AddSingleton<IStringLocalizer<FloatingActionButton>>(new MockStringLocalizer());
        }

        [Fact]
        public void TestDashboard_RendersCorrectly()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert
            Assert.NotNull(component);
            Assert.Contains("mobile-header", component.Markup);
            Assert.Contains("mobile-quick-actions", component.Markup);
            Assert.Contains("dashboard-grid", component.Markup);
        }

        [Fact]
        public void MobileHeader_ContainsRequiredElements()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check for mobile header elements
            var hamburgerMenu = component.Find(".mobile-menu-toggle");
            Assert.NotNull(hamburgerMenu);

            var searchButton = component.Find("button[title*='Search'], button[title*='검색']");
            Assert.NotNull(searchButton);

            var notificationButton = component.Find(".mobile-notifications-btn");
            Assert.NotNull(notificationButton);

            var notificationBadge = component.Find(".notification-badge");
            Assert.NotNull(notificationBadge);
            Assert.Equal("3", notificationBadge.TextContent);
        }

        [Fact]
        public void MobileQuickActions_ContainsAllButtons()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check for quick action buttons
            var quickActionButtons = component.FindAll(".quick-action-btn");
            Assert.Equal(4, quickActionButtons.Count);

            // Check button content
            var buttonTexts = quickActionButtons.Select(b => b.TextContent.Trim()).ToList();
            Assert.Contains("영업", buttonTexts); // Sales
            Assert.Contains("연락처", buttonTexts); // Contacts  
            Assert.Contains("작업", buttonTexts); // Tasks
            Assert.Contains("보고서", buttonTexts); // Reports
        }

        [Fact]
        public void DashboardGrid_ContainsRequiredCards()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check for dashboard cards
            var dashboardCards = component.FindAll(".dashboard-card");
            Assert.True(dashboardCards.Count >= 4, "Should have at least 4 dashboard cards");

            // Check for specific card content
            var cardTexts = string.Join(" ", dashboardCards.Select(c => c.TextContent));
            Assert.Contains("영업 파이프라인", cardTexts); // Sales Pipeline
            Assert.Contains("분기별 실적", cardTexts); // Quarterly Performance
            Assert.Contains("작업", cardTexts); // Tasks
            Assert.Contains("최근 활동", cardTexts); // Recent Activity
        }

        [Fact]
        public void MobileSearchBar_InitiallyCollapsed()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert
            var searchBar = component.Find(".mobile-search-bar");
            Assert.NotNull(searchBar);
            Assert.Contains("collapsed", searchBar.GetAttribute("class") ?? "");
        }

        [Fact]
        public void MobileNotificationsPanel_InitiallyCollapsed()
        {
            // Arrange & Act  
            var component = RenderComponent<TestDashboard>();

            // Assert
            var notificationsPanel = component.Find(".mobile-notifications-panel");
            Assert.NotNull(notificationsPanel);
            Assert.Contains("collapsed", notificationsPanel.GetAttribute("class") ?? "");
        }

        [Fact]
        public void NotificationsPanel_ContainsNotificationItems()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check for notification items
            var notificationItems = component.FindAll(".notification-item");
            Assert.Equal(3, notificationItems.Count);

            // Check notification content
            var notificationTexts = string.Join(" ", notificationItems.Select(n => n.TextContent));
            Assert.Contains("웹사이트에서 새로운 리드", notificationTexts); // New lead from website
            Assert.Contains("거래가 성공적으로 성사되었습니다", notificationTexts); // Deal closed successfully
            Assert.Contains("작업 알림: 후속 조치", notificationTexts); // Task reminder: Follow up
        }

        [Fact]
        public void MobileHeader_HasProperAccessibilityAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check accessibility attributes
            var hamburgerMenu = component.Find(".mobile-menu-toggle");
            Assert.True(hamburgerMenu.HasAttribute("title"));

            var searchButton = component.Find("button[title*='Search'], button[title*='검색']");
            Assert.True(searchButton.HasAttribute("title"));

            var notificationButton = component.Find(".mobile-notifications-btn");
            Assert.True(notificationButton.HasAttribute("title"));
        }

        [Fact]
        public void QuickActionButtons_HaveProperStructure()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check quick action button structure
            var quickActionButtons = component.FindAll(".quick-action-btn");

            foreach (var button in quickActionButtons)
            {
                // Each button should have an SVG icon and text span
                var svg = button.QuerySelector("svg");
                var span = button.QuerySelector("span");
                
                Assert.NotNull(svg);
                Assert.NotNull(span);
                Assert.False(string.IsNullOrEmpty(span?.TextContent));
            }
        }

        [Fact]
        public void DashboardCards_HaveProperClickHandlers()
        {
            // Arrange & Act
            var component = RenderComponent<TestDashboard>();

            // Assert - Check dashboard cards have click handlers
            var dashboardCards = component.FindAll(".dashboard-card");

            foreach (var card in dashboardCards)
            {
                Assert.True(card.HasAttribute("blazor:onclick") || card.HasAttribute("onclick"));
            }
        }

        // Mock classes for testing
        private class MockNavigationManager : NavigationManager
        {
            public MockNavigationManager() : base()
            {
                Initialize("https://localhost/", "https://localhost/test-dashboard");
            }
        }

        private class MockStringLocalizer : IStringLocalizer<MainDashboard>, IStringLocalizer<FloatingActionButton>
        {
            public LocalizedString this[string name] => new(name, GetMockTranslation(name));
            public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(GetMockTranslation(name), arguments));

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => new LocalizedString[0];

            private string GetMockTranslation(string key)
            {
                return key switch
                {
                    "CRM" => "CRM",
                    "Search" => "검색",
                    "Notifications" => "알림",
                    "Sales" => "영업",
                    "Contacts" => "연락처",
                    "Tasks" => "작업",
                    "Reports" => "보고서",
                    "SalesPipeline" => "영업 파이프라인",
                    "QuarterlyPerformance" => "분기별 실적",
                    "RecentActivity" => "최근 활동",
                    "DashboardOverview" => "대시보드 개요",
                    "NewLead" => "웹사이트에서 새로운 리드",
                    "DealClosed" => "거래가 성공적으로 성사되었습니다",
                    "TaskReminder" => "작업 알림: 후속 조치",
                    "MinutesAgo" => "분 전",
                    "HourAgo" => "시간 전",
                    "OpenMenu" => "메뉴 열기",
                    _ => key
                };
            }
        }
    }
}