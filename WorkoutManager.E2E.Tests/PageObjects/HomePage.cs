using Microsoft.Playwright;

namespace WorkoutManager.E2E.Tests.PageObjects;

public class HomePage : BasePage
{
    private const string CreatePlanButtonSelector = "button[aria-label='Add']";
    private const string PlanCardSelector = ".mud-card";
    private const string PlanNameSelector = ".mud-card .mud-typography-h6";
    private const string LoadingSpinnerSelector = ".mud-progress-circular";

    public HomePage(IPage page, string baseUrl = "http://localhost:5000") : base(page, baseUrl)
    {
    }

    public async Task NavigateAsync()
    {
        await NavigateToAsync("/");
        await WaitForLoadingAsync();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        // If we can see the create plan button, we're authenticated
        try
        {
            await Page.WaitForSelectorAsync(CreatePlanButtonSelector, new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ClickCreatePlanButtonAsync()
    {
        await ClickAsync(CreatePlanButtonSelector);
    }

    public async Task<bool> HasPlansAsync()
    {
        var planCards = await Page.Locator(PlanCardSelector).CountAsync();
        return planCards > 0;
    }

    public async Task<int> GetPlanCountAsync()
    {
        await WaitForLoadingAsync();
        return await Page.Locator(PlanCardSelector).CountAsync();
    }

    public async Task<string> GetFirstPlanNameAsync()
    {
        await WaitForLoadingAsync();
        return await GetTextAsync(PlanNameSelector);
    }

    public async Task ClickFirstPlanAsync()
    {
        await WaitForLoadingAsync();
        await ClickAsync(PlanCardSelector);
    }

    public async Task<bool> PlanExistsByNameAsync(string planName)
    {
        await WaitForLoadingAsync();
        var planNames = await Page.Locator(PlanNameSelector).AllTextContentsAsync();
        return planNames.Any(name => name.Contains(planName));
    }

    private async Task WaitForLoadingAsync()
    {
        // Wait for loading spinner to disappear if present
        try
        {
            await Page.WaitForSelectorAsync(LoadingSpinnerSelector, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 5000
            });
        }
        catch
        {
            // If no spinner found, continue
        }
    }
}
