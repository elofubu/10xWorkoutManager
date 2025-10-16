using Microsoft.Playwright;
using Xunit;

namespace WorkoutManager.E2E.Tests;

public class BasePlaywrightTest : IAsyncLifetime
{
    protected string BaseUrl => "http://localhost:5000";
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;


    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();

        await Context.RouteAsync("**/*", async route =>
        {
            await route.ContinueAsync();
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
        Playwright?.Dispose();
    }
}

