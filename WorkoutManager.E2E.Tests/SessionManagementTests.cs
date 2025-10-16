using FluentAssertions;
using Xunit;
using WorkoutManager.E2E.Tests.Helpers;
using WorkoutManager.E2E.Tests.PageObjects;

namespace WorkoutManager.E2E.Tests;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class SessionManagementTests : BasePlaywrightTest
{
    [Fact(Skip = "Requires existing workout plan with exercises - manual setup needed")]
    public async Task StartWorkout_WithActiveSession_ShouldShowDialog()
    {
        // This test verifies the business rule: only one active session at a time
        // When a user tries to start a workout while another is active,
        // they should see a dialog with options to:
        // - Continue existing session
        // - Finish current and start new

        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);
        var planDetailPage = new PlanDetailPage(Page, BaseUrl);

        // Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Navigate to home page
        await homePage.NavigateAsync();

        // TODO:
        // 1. Start a workout (first session)
        // 2. Navigate back without finishing
        // 3. Try to start another workout
        // 4. Verify dialog appears with "continue" and "finish & start new" options
        // 5. Test both options work correctly

        // Implementation requires:
        // - Existing plan with exercises
        // - Dialog interaction helpers
        // - Session state verification
    }

    [Fact(Skip = "Requires dialog interaction helpers")]
    public async Task ResumeSession_ShouldContinueFromWhereLeft()
    {
        // This test verifies that when a user has an active session and navigates away,
        // they can resume the session and continue from where they left off

        // Test flow:
        // 1. Login
        // 2. Start workout
        // 3. Fill some exercise data
        // 4. Navigate away (without finishing)
        // 5. Navigate back to plans
        // 6. Dialog should appear with option to continue
        // 7. Choose continue
        // 8. Verify previous data is still there
    }

    [Fact(Skip = "Requires API integration for session state verification")]
    public async Task CancelWorkout_ShouldNotSaveSession()
    {
        // This test verifies that canceling a workout doesn't save it to history

        // Test flow:
        // 1. Login
        // 2. Start workout
        // 3. Fill some data
        // 4. Click "Cancel workout"
        // 5. Verify session is not in history
        // 6. Verify no active session exists
    }

    [Fact]
    public async Task Navigation_BetweenPages_ShouldMaintainAuthentication()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);
        var historyPage = new HistoryPage(Page, BaseUrl);

        // Act & Assert
        // Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Navigate to home
        await homePage.NavigateAsync();
        var isAuthOnHome = await homePage.IsAuthenticatedAsync();
        isAuthOnHome.Should().BeTrue("should be authenticated on home page");

        // Navigate to history
        await historyPage.NavigateAsync();
        var isAuthOnHistory = await historyPage.IsHistoryPageAsync();
        isAuthOnHistory.Should().BeTrue("should be authenticated on history page");

        // Navigate back to home
        await homePage.NavigateAsync();
        var isStillAuth = await homePage.IsAuthenticatedAsync();
        isStillAuth.Should().BeTrue("should still be authenticated after navigation");
    }
}
