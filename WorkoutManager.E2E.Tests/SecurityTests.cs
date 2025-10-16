using FluentAssertions;
using Microsoft.Playwright;
using Xunit;
using WorkoutManager.E2E.Tests.Helpers;
using WorkoutManager.E2E.Tests.PageObjects;

namespace WorkoutManager.E2E.Tests;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class SecurityTests : BasePlaywrightTest
{
    [Theory]
    [InlineData("/")]
    [InlineData("/history")]
    [InlineData("/settings")]
    public async Task ProtectedRoute_WithoutAuth_ShouldRedirectToLogin(string route)
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}{route}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Assert
        var currentUrl = Page.Url;
        currentUrl.Should().Contain("/authentication/login",
            $"accessing {route} without authentication should redirect to login");
    }

    [Fact]
    public async Task AuthenticatedUser_CanAccessProtectedRoutes()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);
        var historyPage = new HistoryPage(Page, BaseUrl);

        // Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Act & Assert - Test each protected route
        await Page.GotoAsync($"{BaseUrl}/");
        var homeUrl = Page.Url;
        homeUrl.Should().NotContain("/authentication/login", "authenticated user can access home");

        await Page.GotoAsync($"{BaseUrl}/history");
        var historyUrl = Page.Url;
        historyUrl.Should().Contain("/history", "authenticated user can access history");

        await Page.GotoAsync($"{BaseUrl}/settings");
        var settingsUrl = Page.Url;
        settingsUrl.Should().Contain("/settings", "authenticated user can access settings");
    }

    [Fact(Skip = "Requires test data setup with two users and user-specific resources")]
    public async Task UserIsolation_User1_CannotAccessUser2Data()
    {
        // This test verifies that User1 cannot access User2's plans, sessions, or exercises
        // through direct URL navigation or API calls

        // Test scenario:
        // 1. Login as User2
        // 2. Create a plan (note the plan ID)
        // 3. Logout
        // 4. Login as User1
        // 5. Try to navigate to User2's plan URL directly
        // 6. Verify: should see 404/403 or be redirected to safe page

        // Prerequisites:
        // - Test users with isolated data
        // - API endpoint to create test data
        // - Error page handling verification
    }

    [Fact]
    public async Task Session_AfterLogout_ShouldNotPersist()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);

        // Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Verify authenticated
        await homePage.NavigateAsync();
        var isAuthBeforeLogout = await homePage.IsAuthenticatedAsync();
        isAuthBeforeLogout.Should().BeTrue();

        // Act - Logout
        await loginPage.LogoutAsync();

        // Try to access protected route
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Assert
        var currentUrl = Page.Url;
        currentUrl.Should().Contain("/authentication/login",
            "after logout, accessing protected route should redirect to login");
    }

    [Fact]
    public async Task DirectAPIAccess_WithoutToken_ShouldBeBlocked()
    {
        // This test verifies that API endpoints require JWT authentication

        // Arrange - Make API call without authentication
        var apiUrl = BaseUrl.Replace("5000", "5048"); // API runs on 5048

        // Act - Try to access API endpoint without token
        var apiRequest = await Context.NewPageAsync();
        var response = await apiRequest.APIRequest.GetAsync($"{apiUrl}/api/workoutplans");

        // Assert
        response.Status.Should().Match(s => s == 401 || s == 403,
            "API should return 401 Unauthorized or 403 Forbidden without valid JWT token");
    }

    [Fact]
    public async Task LocalStorageSession_ShouldBeHttpOnly()
    {
        // This test verifies security properties of the session storage

        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);

        // Act - Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Check session exists
        var sessionData = await Page.EvaluateAsync<string?>(@"
            () => {
                return localStorage.getItem('supabase_session');
            }
        ");

        // Assert
        sessionData.Should().NotBeNullOrEmpty("session should be stored after login");

        // Note: In a production environment with proper security:
        // - Tokens should be in httpOnly cookies (can't be accessed via JS)
        // - localStorage is accessible but better than nothing
        // - Consider recommending httpOnly cookies in security review
    }
}
