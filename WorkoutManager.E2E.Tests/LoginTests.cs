using FluentAssertions;
using Microsoft.Playwright;
using Xunit;
using WorkoutManager.E2E.Tests.Helpers;
using WorkoutManager.E2E.Tests.PageObjects;

namespace WorkoutManager.E2E.Tests;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class AuthenticationTests : BasePlaywrightTest
{
    [Fact]
    public async Task LoginPage_ShouldBeAccessible()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);

        // Act
        await loginPage.NavigateAsync();

        // Assert
        var isVisible = await loginPage.IsLoginFormVisibleAsync();
        isVisible.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);

        // Act
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);

        // Wait for navigation to complete
        await Page.WaitForURLAsync(url => url.Contains(BaseUrl) && !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Assert
        var isAuthenticated = await homePage.IsAuthenticatedAsync();
        isAuthenticated.Should().BeTrue("user should be authenticated after successful login");

        var currentUrl = Page.Url;
        currentUrl.Should().NotContain("/authentication/login", "user should be redirected away from login page");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldShowError()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);

        // Act
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.InvalidUser.Email, TestData.Users.InvalidUser.Password);

        // Wait a bit for error message to appear
        await Page.WaitForTimeoutAsync(2000);

        // Assert
        var hasError = await loginPage.HasErrorMessageAsync();
        hasError.Should().BeTrue("error message should be displayed for invalid credentials");

        var isStillOnLoginPage = await loginPage.IsLoginFormVisibleAsync();
        isStillOnLoginPage.Should().BeTrue("user should remain on login page after failed login");
    }

    [Fact]
    public async Task Logout_ShouldRedirectToLoginPage()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);
        var homePage = new HomePage(Page, BaseUrl);

        // Login first
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Act - Logout
        await loginPage.LogoutAsync();

        // Assert
        var currentUrl = Page.Url;
        currentUrl.Should().Contain("/authentication/login", "user should be redirected to login page after logout");

        var isLoginFormVisible = await loginPage.IsLoginFormVisibleAsync();
        isLoginFormVisible.Should().BeTrue("login form should be visible after logout");
    }

    [Fact]
    public async Task ProtectedRoute_WithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);

        // Act - Try to access protected route (home page) without logging in
        await Page.GotoAsync(BaseUrl);

        // Wait for potential redirect
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(2000);

        // Assert
        var currentUrl = Page.Url;
        currentUrl.Should().Contain("/authentication/login", "unauthenticated user should be redirected to login page");
    }

    [Fact]
    public async Task Login_ShouldPersistSession_InLocalStorage()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);

        // Act
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Check for session in localStorage
        var sessionExists = await Page.EvaluateAsync<bool>(@"
            () => {
                const session = localStorage.getItem('supabase_session');
                return session !== null && session !== undefined;
            }
        ");

        // Assert
        sessionExists.Should().BeTrue("session should be stored in localStorage after successful login");
    }

    [Fact]
    public async Task Logout_ShouldClearSession_FromLocalStorage()
    {
        // Arrange
        var loginPage = new LoginPage(Page, BaseUrl);

        // Login first
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync(TestData.Users.User1.Email, TestData.Users.User1.Password);
        await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 10000 });

        // Act - Logout
        await loginPage.LogoutAsync();
        await Page.WaitForTimeoutAsync(1000);

        // Check localStorage
        var sessionCleared = await Page.EvaluateAsync<bool>(@"
            () => {
                const session = localStorage.getItem('supabase_session');
                return session === null || session === undefined || session === '';
            }
        ");

        // Assert
        sessionCleared.Should().BeTrue("session should be cleared from localStorage after logout");
    }
}

