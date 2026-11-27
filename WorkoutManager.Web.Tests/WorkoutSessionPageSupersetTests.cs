using Blazored.LocalStorage;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Helpers;
using WorkoutManager.Web.Pages.Session;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Tests;

/// <summary>
/// Behavior of the workout page when another exercise is pinned next to the current one
/// (superset / combined sets).
/// </summary>
public class WorkoutSessionPageSupersetTests : TestContext
{
    private readonly FakeSessionService _sessions = new();
    private readonly InMemoryStateService _state = new();
    private IRenderedComponent<MudPopoverProvider> _popovers = default!;

    public WorkoutSessionPageSupersetTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddBlazoredLocalStorage();
        Services.AddSingleton<ISessionService>(_sessions);
        Services.AddSingleton<IExerciseService, FakeExerciseService>();
        Services.AddSingleton<IWorkoutSessionStateService>(_state);
    }

    private IRenderedComponent<WorkoutSessionPage> RenderPage()
    {
        _popovers = RenderComponent<MudPopoverProvider>();
        RenderComponent<MudDialogProvider>();
        RenderComponent<MudSnackbarProvider>();

        var page = RenderComponent<WorkoutSessionPage>(p => p.Add(x => x.SessionId, 10));
        page.WaitForAssertion(() => page.FindAll(".log-panel").Should().HaveCount(1));
        return page;
    }

    private async Task PinAsync(IRenderedComponent<WorkoutSessionPage> page, string label)
    {
        page.Find(".session-pin-bar button").Click();
        var item = _popovers.WaitForElements(".mud-menu-item")
            .First(e => e.TextContent.Contains(label));
        await item.ClickAsync(new());
        page.WaitForAssertion(() => page.FindAll(".log-panel-pinned").Should().HaveCount(1));
    }

    private static void ClickButton(IRenderedComponent<WorkoutSessionPage> page, string text, int index = 0) =>
        page.FindAll("button").Where(b => b.TextContent.Trim() == text).ElementAt(index).Click();

    [Fact]
    public void Pin_menu_offers_every_other_exercise_not_only_the_next_one()
    {
        var page = RenderPage();

        page.Find(".session-pin-bar button").Click();

        var items = _popovers.WaitForElements(".mud-menu-item").Select(e => e.TextContent.Trim()).ToList();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.Contains("Squats"));
        items.Should().Contain(i => i.Contains("Pull-ups"));
        items.Should().NotContain(i => i.Contains("Bench Press"), "the current exercise cannot be pinned");
    }

    [Fact]
    public async Task Pinned_exercise_gets_its_own_panel_and_sets()
    {
        var page = RenderPage();

        await PinAsync(page, "Squats");
        page.Find(".log-panel-pinned .mud-typography-h5").TextContent.Should().Contain("Squats");

        // Log a set on the pinned exercise only.
        page.FindAll("[aria-label='Increase reps']")[1].Click();
        ClickButton(page, "Add set", index: 1);

        page.FindAll(".log-panel-pinned .set-item").Should().HaveCount(1);
        page.FindAll(".log-panel:not(.log-panel-pinned) .set-item").Should().BeEmpty();
    }

    [Fact]
    public void Add_set_starts_the_rest_timer()
    {
        var page = RenderPage();
        page.FindAll(".rest-timer-time").Should().BeEmpty();

        page.FindAll("[aria-label='Increase reps']")[0].Click();
        ClickButton(page, "Add set");

        page.Find(".rest-timer-time").TextContent.Should().Be("2:00");
    }

    [Fact]
    public async Task In_a_superset_the_rest_timer_starts_after_the_pinned_exercise_set()
    {
        var page = RenderPage();
        await PinAsync(page, "Squats");

        page.FindAll("[aria-label='Increase reps']")[0].Click();
        ClickButton(page, "Add set", index: 0);
        page.FindAll(".rest-timer-time").Should().BeEmpty("the pinned exercise of this round is still to do");

        page.FindAll("[aria-label='Increase reps']")[1].Click();
        ClickButton(page, "Add set", index: 1);
        page.FindAll(".rest-timer-time").Should().HaveCount(1);

        // Next keeps the pin: on the new exercise the round again ends with the pinned one.
        page.Find("[aria-label='Stop rest timer']").Click();
        ClickButton(page, "Next");
        page.WaitForAssertion(() =>
            page.Find(".log-panel:not(.log-panel-pinned) .mud-typography-h5").TextContent.Should().Contain("Pull-ups"));

        page.FindAll("[aria-label='Increase reps']")[0].Click();
        ClickButton(page, "Add set", index: 0);
        page.FindAll(".rest-timer-time").Should().BeEmpty();

        page.FindAll("[aria-label='Increase reps']")[1].Click();
        ClickButton(page, "Add set", index: 1);
        page.FindAll(".rest-timer-time").Should().HaveCount(1);
    }

    [Fact]
    public async Task Next_saves_both_the_current_and_the_pinned_exercise_and_keeps_the_pin()
    {
        var page = RenderPage();
        await PinAsync(page, "Squats");

        page.FindAll("[aria-label='Increase reps']")[0].Click();
        ClickButton(page, "Add set", index: 0);
        page.FindAll("[aria-label='Increase reps']")[1].Click();
        ClickButton(page, "Add set", index: 1);

        ClickButton(page, "Next");

        page.WaitForAssertion(() =>
        {
            _sessions.Updates.Select(u => u.SessionExerciseId).Should().BeEquivalentTo(new long[] { 1, 3 });
            _sessions.Updates.Single(u => u.SessionExerciseId == 1).Payload.Sets.Should().ContainSingle();
            _sessions.Updates.Single(u => u.SessionExerciseId == 3).Payload.Sets.Should().ContainSingle();
        });

        // Now on exercise 2; Squats is still pinned.
        page.FindAll(".log-panel-pinned").Should().HaveCount(1);
        page.Find(".log-panel:not(.log-panel-pinned) .mud-typography-h5").TextContent.Should().Contain("Pull-ups");
    }

    [Fact]
    public async Task Pin_is_released_when_the_pinned_exercise_becomes_the_current_one()
    {
        var page = RenderPage();
        await PinAsync(page, "Pull-ups");

        ClickButton(page, "Next");

        page.WaitForAssertion(() =>
            page.Find(".log-panel .mud-typography-h5").TextContent.Should().Contain("Pull-ups"));
        page.FindAll(".log-panel-pinned").Should().BeEmpty();
        page.FindAll(".session-pin-bar").Should().HaveCount(1, "another exercise can be pinned again");
    }

    [Fact]
    public async Task Unpin_saves_the_pinned_exercise_and_removes_its_panel()
    {
        var page = RenderPage();
        await PinAsync(page, "Squats");
        page.FindAll("[aria-label='Increase reps']")[1].Click();
        ClickButton(page, "Add set", index: 1);

        page.Find("[aria-label='Unpin exercise']").Click();

        page.WaitForAssertion(() => page.FindAll(".log-panel-pinned").Should().BeEmpty());
        _sessions.Updates.Should().ContainSingle(u => u.SessionExerciseId == 3)
            .Which.Payload.Sets.Should().ContainSingle();
    }

    [Fact]
    public async Task Pin_and_its_sets_survive_a_page_refresh()
    {
        var page = RenderPage();
        await PinAsync(page, "Squats");
        page.FindAll("[aria-label='Increase reps']")[1].Click();
        ClickButton(page, "Add set", index: 1);
        _state.Current.Should().NotBeNull();
        _state.Current!.PinnedExerciseId.Should().Be(3);

        // "Refresh": a brand new page instance reading the same saved state.
        _sessions.Reset();
        var refreshed = RenderComponent<WorkoutSessionPage>(p => p.Add(x => x.SessionId, 10));

        refreshed.WaitForAssertion(() =>
        {
            refreshed.FindAll(".log-panel-pinned").Should().HaveCount(1);
            refreshed.FindAll(".log-panel-pinned .set-item").Should().HaveCount(1);
        });
    }

    [Fact]
    public void Removing_a_set_removes_the_one_that_was_clicked()
    {
        var page = RenderPage();

        page.FindAll("[aria-label='Increase reps']")[0].Click();
        ClickButton(page, "Add set");            // set 1: 0 kg x 1
        page.FindAll("[aria-label='Increase weight']")[0].Click();
        ClickButton(page, "Add set");            // set 2: 2.5 kg x 1

        page.FindAll("[aria-label='Remove set']")[1].Click();

        var remaining = page.FindAll(".log-panel .set-item-main");
        remaining.Should().HaveCount(1);
        remaining[0].TextContent.Should().NotContain("2.5");
    }

    [Fact]
    public void Note_typed_for_an_exercise_is_kept_in_storage_and_saved_on_Next()
    {
        var page = RenderPage();

        ClickButton(page, "Add note");
        page.Find(".log-note textarea").Input("Grip gave out on the last set");
        _state.Current!.UnsavedNotes.Should().Be("Grip gave out on the last set");

        ClickButton(page, "Next");

        page.WaitForAssertion(() =>
            _sessions.Updates.Single(u => u.SessionExerciseId == 1).Payload.Notes
                .Should().Be("Grip gave out on the last set"));
    }

    [Fact]
    public void Add_set_needs_at_least_one_rep_but_allows_zero_weight()
    {
        var page = RenderPage();
        Button(page, "Add set").HasAttribute("disabled").Should().BeTrue("0 reps is not a set");

        page.FindAll("[aria-label='Increase reps']")[0].Click();
        Button(page, "Add set").HasAttribute("disabled").Should().BeFalse();
        ClickButton(page, "Add set");

        page.FindAll(".log-panel .set-item").Should().ContainSingle("a bodyweight set (0 kg) is fine");
    }

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<WorkoutSessionPage> page, string text) =>
        page.FindAll("button").First(b => b.TextContent.Trim() == text);

    // ---- test doubles -------------------------------------------------------------------------

    private sealed class FakeSessionService : ISessionService
    {
        public List<(long SessionExerciseId, UpdateSessionExerciseDto Payload)> Updates { get; } = new();

        public void Reset() => Updates.Clear();

        public Task<SessionDetailsDto> GetSessionDetailsAsync(long id) => Task.FromResult(new SessionDetailsDto
        {
            Id = id,
            TrainingDayId = 5,
            Exercises = new List<SessionExerciseDetailsDto>
            {
                new() { Id = 1, ExerciseId = 101, ExerciseName = "Bench Press", Order = 1, Sets = new() },
                new() { Id = 2, ExerciseId = 102, ExerciseName = "Pull-ups", Order = 2, Sets = new() },
                new() { Id = 3, ExerciseId = 103, ExerciseName = "Squats", Order = 3, Sets = new() }
            }
        });

        public Task UpdateSessionExerciseAsync(long sessionId, long sessionExerciseId, UpdateSessionExerciseDto payload)
        {
            Updates.Add((sessionExerciseId, payload));
            return Task.CompletedTask;
        }

        public Task FinishSessionAsync(long sessionId, string? notes) => Task.CompletedTask;

        public Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync() => throw new NotSupportedException();
        public Task<SessionDetailsDto> StartSessionAsync(long trainingDayId) => throw new NotSupportedException();
        public Task UpdateSessionAsync(long sessionId, UpdateSessionCommand command) => throw new NotSupportedException();
        public Task<SessionDetailsDto?> GetActiveSessionAsync() => throw new NotSupportedException();
        public Task DeleteSessionAsync(long sessionId) => throw new NotSupportedException();
    }

    private sealed class FakeExerciseService : IExerciseService
    {
        public Task<PreviousExercisePerformanceDto?> GetPreviousSessionExerciseAsync(long exerciseId, long trainingDayId) =>
            Task.FromResult<PreviousExercisePerformanceDto?>(null);

        public Task<ExerciseDto?> GetExerciseByIdAsync(long exerciseId) =>
            Task.FromResult<ExerciseDto?>(new ExerciseDto { Id = exerciseId, Name = $"Exercise {exerciseId}" });

        public Task<PaginatedList<ExerciseDto>> GetExercisesAsync(string? search = null, long? muscleGroupId = null, int page = 1) =>
            throw new NotSupportedException();

        public Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto newExercise) => throw new NotSupportedException();
    }

    private sealed class InMemoryStateService : IWorkoutSessionStateService
    {
        public WorkoutSessionState? Current { get; private set; }

        public Task SaveStateAsync(WorkoutSessionState state)
        {
            Current = state;
            return Task.CompletedTask;
        }

        public Task<WorkoutSessionState?> LoadStateAsync() => Task.FromResult(Current);

        public Task ClearStateAsync()
        {
            Current = null;
            return Task.CompletedTask;
        }
    }
}
