using Microsoft.Playwright;

namespace WorkoutManager.E2E.Tests.PageObjects;

public class LoginPage : BasePage
{
    private const string EmailInputSelector = "input[type='email']";
    private const string PasswordInputSelector = "input[type='password']";
    //private const string LoginButtonSelector = "button:has-text('Sing In')";
    private const string LoginButtonSelector = "button[type='button']";
    private const string ErrorMessageSelector = ".mud-alert-message";
    private const string LogoutButtonSelector = "button[aria-label='Logout']";

    public LoginPage(IPage page, string baseUrl = "http://localhost:5000") : base(page, baseUrl)
    {
    }

    public async Task NavigateAsync()
    {
        await NavigateToAsync("/authentication/login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task LoginAsync(string email, string password)
    {
        await FillAsync(EmailInputSelector, email);
        await FillAsync(PasswordInputSelector, password);
        await ClickAsync(LoginButtonSelector);

        // Wait for either navigation away from login or error message
        try
        {
            await Page.WaitForURLAsync(url => !url.Contains("/authentication/login"), new() { Timeout = 5000 });
        }
        catch
        {
            // May have stayed on login page due to error
        }
    }

    public async Task<bool> IsLoginFormVisibleAsync()
    {
        return await IsElementVisibleAsync(EmailInputSelector);
    }

    public async Task<bool> HasErrorMessageAsync()
    {
        return await IsElementVisibleAsync(ErrorMessageSelector);
    }

    public async Task<string?> GetErrorMessageAsync()
    {
        if (await HasErrorMessageAsync())
        {
            return await GetTextAsync(ErrorMessageSelector);
        }
        return null;
    }

    public async Task LogoutAsync()
    {
        var logoutButton = Page.Locator(LogoutButtonSelector);
        if (await logoutButton.CountAsync() > 0)
        {
            await logoutButton.ClickAsync();
            await Page.WaitForURLAsync("**/authentication/login", new() { Timeout = 5000 });
        }
    }
}



