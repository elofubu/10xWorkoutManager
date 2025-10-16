using Microsoft.Playwright;

namespace WorkoutManager.E2E.Tests.PageObjects;

public class HistoryPage : BasePage
{
    private const string PageTitleSelector = ".mud-typography-h4";
    private const string LoadingSpinnerSelector = ".mud-progress-circular";
    private const string SessionListItemSelector = ".mud-list-item";
    private const string NoSessionsTextSelector = "p:has-text('You haven\\'t completed any workouts yet')";

    public HistoryPage(IPage page, string baseUrl = "http://localhost:5000") : base(page, baseUrl)
    {
    }

    public async Task NavigateAsync()
    {
        await NavigateToAsync("/history");
        await WaitForLoadingAsync();
    }

    public async Task<bool> IsHistoryPageAsync()
    {
        var title = await GetTextAsync(PageTitleSelector);
        return title.Contains("Workout History");
    }

    public async Task<bool> HasSessionsAsync()
    {
        await WaitForLoadingAsync();
        var noSessionsVisible = await IsElementVisibleAsync(NoSessionsTextSelector);
        return !noSessionsVisible;
    }

    public async Task<int> GetSessionCountAsync()
    {
        await WaitForLoadingAsync();

        // Check if "no sessions" message is visible
        if (await IsElementVisibleAsync(NoSessionsTextSelector))
        {
            return 0;
        }

        return await Page.Locator(SessionListItemSelector).CountAsync();
    }

    public async Task<bool> SessionExistsWithPlanNameAsync(string planName)
    {
        await WaitForLoadingAsync();

        if (await GetSessionCountAsync() == 0)
        {
            return false;
        }

        var sessionTexts = await Page.Locator(SessionListItemSelector).AllTextContentsAsync();
        return sessionTexts.Any(text => text.Contains(planName));
    }

    public async Task ClickFirstSessionAsync()
    {
        await WaitForLoadingAsync();
        await Page.Locator(SessionListItemSelector).First.ClickAsync();
    }

    private async Task WaitForLoadingAsync()
    {
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
