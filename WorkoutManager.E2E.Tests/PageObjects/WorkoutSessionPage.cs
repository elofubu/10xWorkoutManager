using Microsoft.Playwright;

namespace WorkoutManager.E2E.Tests.PageObjects;

public class WorkoutSessionPage : BasePage
{
    private const string LoadingSpinnerSelector = ".mud-progress-circular";
    private const string WeightInputSelector = "input[aria-label*='Weight']";
    private const string RepsInputSelector = "input[aria-label*='Reps']";
    private const string FailureCheckboxSelector = "input[type='checkbox']:near(label:has-text('To Failure'))";
    private const string AddSetButtonSelector = "button:has-text('Add Set')";
    private const string SaveAndNextButtonSelector = "button:has-text('Save and Next')";
    private const string FinishWorkoutButtonSelector = "button:has-text('Finish Workout')";
    private const string CancelWorkoutButtonSelector = "button:has-text('Cancel workout')";
    private const string SessionNotesSelector = "textarea[aria-label*='Session Notes']";
    private const string ExerciseNotesSelector = "textarea[aria-label*='Notes']";
    private const string StepSelector = ".mud-step";

    public WorkoutSessionPage(IPage page, string baseUrl = "http://localhost:5000") : base(page, baseUrl)
    {
    }

    public async Task NavigateAsync(int sessionId)
    {
        await NavigateToAsync($"/session/workout/{sessionId}");
        await WaitForLoadingAsync();
    }

    public async Task<bool> IsSessionLoadedAsync()
    {
        await WaitForLoadingAsync();
        var stepCount = await Page.Locator(StepSelector).CountAsync();
        return stepCount > 0;
    }

    public async Task<int> GetExerciseStepCountAsync()
    {
        await WaitForLoadingAsync();
        return await Page.Locator(StepSelector).CountAsync();
    }

    public async Task FillSetDataAsync(int setIndex, decimal weight, int reps, bool toFailure = false)
    {
        var weightInputs = Page.Locator(WeightInputSelector);
        var repsInputs = Page.Locator(RepsInputSelector);

        await weightInputs.Nth(setIndex).FillAsync(weight.ToString());
        await repsInputs.Nth(setIndex).FillAsync(reps.ToString());

        if (toFailure)
        {
            var failureCheckboxes = Page.Locator(FailureCheckboxSelector);
            await failureCheckboxes.Nth(setIndex).CheckAsync();
        }
    }

    public async Task ClickAddSetAsync()
    {
        await Page.Locator(AddSetButtonSelector).First.ClickAsync();
        await Page.WaitForTimeoutAsync(300); // Wait for new set to appear
    }

    public async Task ClickSaveAndNextAsync()
    {
        var saveButton = Page.Locator(SaveAndNextButtonSelector);
        if (await saveButton.CountAsync() > 0)
        {
            await saveButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500); // Wait for navigation to next step
        }
    }

    public async Task ClickFinishWorkoutAsync()
    {
        var finishButton = Page.Locator(FinishWorkoutButtonSelector);
        if (await finishButton.CountAsync() > 0)
        {
            await finishButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    public async Task ClickCancelWorkoutAsync()
    {
        await Page.Locator(CancelWorkoutButtonSelector).ClickAsync();
    }

    public async Task FillSessionNotesAsync(string notes)
    {
        await Page.Locator(SessionNotesSelector).FillAsync(notes);
    }

    public async Task FillExerciseNotesAsync(string notes)
    {
        await Page.Locator(ExerciseNotesSelector).First.FillAsync(notes);
    }

    public async Task<bool> HasFinishWorkoutButtonAsync()
    {
        var count = await Page.Locator(FinishWorkoutButtonSelector).CountAsync();
        return count > 0;
    }

    public async Task<bool> HasSaveAndNextButtonAsync()
    {
        var count = await Page.Locator(SaveAndNextButtonSelector).CountAsync();
        return count > 0;
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
