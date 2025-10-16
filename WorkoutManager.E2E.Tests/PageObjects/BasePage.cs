using Microsoft.Playwright;

namespace WorkoutManager.E2E.Tests.PageObjects;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl = "http://localhost:5000")
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    protected async Task NavigateToAsync(string relativeUrl)
    {
        await Page.GotoAsync($"{BaseUrl}{relativeUrl}");
    }

    protected async Task<bool> IsElementVisibleAsync(string selector)
    {
        return await Page.Locator(selector).IsVisibleAsync();
    }

    protected async Task ClickAsync(string selector)
    {
        await Page.Locator(selector).ClickAsync();
    }

    protected async Task FillAsync(string selector, string value)
    {
        await Page.Locator(selector).FillAsync(value);
    }

    protected async Task<string> GetTextAsync(string selector)
    {
        return await Page.Locator(selector).TextContentAsync() ?? string.Empty;
    }
}



