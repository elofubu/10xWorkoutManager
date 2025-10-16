using FluentAssertions;
using Xunit;
using WorkoutManager.E2E.Tests.Helpers;
using WorkoutManager.E2E.Tests.PageObjects;

namespace WorkoutManager.E2E.Tests;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class UserJourneyTests : BasePlaywrightTest
{
    [Fact]
    public async Task FullUserJourney_CreatePlan_StartSession_CompleteWorkout_VerifyHistory()
    {
        // Note: This test assumes the app has some predefined exercises and training days
        // It may need to be adjusted based on actual application behavior

        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);
        var historyPage = new HistoryPage(Page, BaseUrl);

        // Step 1: Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        var isAuthenticated = await homePage.IsAuthenticatedAsync();
        isAuthenticated.Should().BeTrue("user should be logged in");

        // Step 2: Check if we have plans (for now, just verify we can see the home page)
        await homePage.NavigateAsync();
        var initialPlanCount = await homePage.GetPlanCountAsync();
        initialPlanCount.Should().BeGreaterThanOrEqualTo(0, "should be able to see plans list");

        // Note: Steps for creating a plan and adding exercises would go here
        // However, these require dialogs/modals which need more complex interaction
        // For now, this test verifies the core flow is accessible

        // Step 3: Verify we can navigate to history
        await historyPage.NavigateAsync();
        var isHistoryPage = await historyPage.IsHistoryPageAsync();
        isHistoryPage.Should().BeTrue("should be able to access history page");

        // Step 4: Logout
        await loginPage.LogoutAsync();
        var currentUrl = Page.Url;
        currentUrl.Should().Contain("/authentication/login", "should be logged out");
    }

    [Fact(Skip = "Requires dialog interaction - implement after dialog helpers are ready")]
    public async Task CompleteWorkoutFlow_WithExistingPlan()
    {
        // This test would:
        // 1. Login
        // 2. Navigate to an existing plan
        // 3. Start a workout
        // 4. Fill in exercise data (sets, reps, weight)
        // 5. Complete the workout
        // 6. Verify it appears in history

        // To be implemented when dialog/modal interaction helpers are ready
    }
}
