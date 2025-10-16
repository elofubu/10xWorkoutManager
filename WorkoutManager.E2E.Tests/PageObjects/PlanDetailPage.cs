using Microsoft.Playwright;

namespace WorkoutManager.E2E.Tests.PageObjects;

public class PlanDetailPage : BasePage
{
    private const string PlanNameSelector = ".mud-typography-h4";
    private const string EditModeToggleSelector = "input[type='checkbox']";
    private const string AddExerciseButtonSelector = "button:has-text('Add Exercise')";
    private const string StartWorkoutButtonSelector = "button:has-text('Start Workout')";
    private const string ExpansionPanelSelector = ".mud-expand-panel";
    private const string ExerciseListItemSelector = ".mud-list-item";
    private const string DeletePlanButtonSelector = "button[aria-label='Delete']";
    private const string LoadingSpinnerSelector = ".mud-progress-circular";

    public PlanDetailPage(IPage page, string baseUrl = "http://localhost:5000") : base(page, baseUrl)
    {
    }

    public async Task NavigateAsync(int planId)
    {
        await NavigateToAsync($"/plans/{planId}");
        await WaitForLoadingAsync();
    }

    public async Task<string> GetPlanNameAsync()
    {
        await WaitForLoadingAsync();
        return await GetTextAsync(PlanNameSelector);
    }

    public async Task<bool> HasTrainingDaysAsync()
    {
        await WaitForLoadingAsync();
        var panels = await Page.Locator(ExpansionPanelSelector).CountAsync();
        return panels > 0;
    }

    public async Task<int> GetTrainingDayCountAsync()
    {
        await WaitForLoadingAsync();
        return await Page.Locator(ExpansionPanelSelector).CountAsync();
    }

    public async Task ExpandFirstDayAsync()
    {
        await WaitForLoadingAsync();
        await Page.Locator(ExpansionPanelSelector).First.ClickAsync();
    }

    public async Task ClickAddExerciseAsync()
    {
        await ClickAsync(AddExerciseButtonSelector);
    }

    public async Task ClickStartWorkoutForFirstDayAsync()
    {
        await WaitForLoadingAsync();
        // Wait for and click the first Start Workout button
        await Page.Locator(StartWorkoutButtonSelector).First.ClickAsync();
    }

    public async Task<bool> HasStartWorkoutButtonAsync()
    {
        await WaitForLoadingAsync();
        var count = await Page.Locator(StartWorkoutButtonSelector).CountAsync();
        return count > 0;
    }

    public async Task EnableEditModeAsync()
    {
        await WaitForLoadingAsync();
        var checkbox = Page.Locator(EditModeToggleSelector);
        var isChecked = await checkbox.IsCheckedAsync();
        if (!isChecked)
        {
            await checkbox.CheckAsync();
        }
    }

    public async Task<int> GetExerciseCountInFirstDayAsync()
    {
        await WaitForLoadingAsync();
        await ExpandFirstDayAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for expansion animation
        return await Page.Locator(ExerciseListItemSelector).CountAsync();
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
