# Loading/Progress Indicator Implementation Plan for WorkoutManager.Web

## Overview

This plan provides a comprehensive strategy for adding loading/progress indicators using **MudBlazor** across the WorkoutManager.Web Blazor WebAssembly project. The project has **21+ async operations** spread across **20+ files** that need appropriate loading/progress indicators.

**Current Status:** Only 3 pages have basic loading indicators; button submissions lack disabled states during processing.

---

## PART 1: Current State Analysis

| Category | Current Implementation | Gaps |
|----------|------------------------|------|
| **Page-level loading** | Basic `null` checks with MudProgressCircular | Missing on 9 pages; no skeleton loaders |
| **Button submissions** | No disabled state during async operations | Double-submit possible; no visual feedback |
| **Dialog operations** | No loading indicators during form submission | User confusion on slow APIs |
| **List pagination** | No loading indicator on page change | Silent data refresh |
| **Edit operations** | `_isEditMode` flag only; no visual feedback | No button disable during save |
| **Error handling** | Snackbar notifications for errors | No retry mechanisms |
| **Search/Filter** | No loading indicator on filter change | Pagination loading not visible |

**Missing in Current Stack:**
- No skeleton loaders (PlaceholderContent components)
- No linear progress bars for sequential operations
- No disabled state management for buttons
- No circular indicators for button loading
- No optimistic UI updates

---

## PART 2: MudBlazor Components to Use

| Component | Use Case | Import |
|-----------|----------|--------|
| **MudProgressCircular** | Full-page data loading | MudBlazor |
| **MudProgressLinear** | Multi-step operations or sequential API calls | MudBlazor |
| **MudButton with Loading** | Form submissions and single actions | MudBlazor |
| **MudSkeleton** | Placeholder content while loading | MudBlazor |
| **MudProgressBar** | Background operations | MudBlazor |
| **MudOverlay** | Prevent user interaction during loading | MudBlazor |

---

## PART 3: Implementation Strategy by File Category

### A. AUTHENTICATION PAGES (5 files)

**Current State:** `_isSubmitting` flag exists but not fully utilized

| File | Method | Current | Recommended |
|------|--------|---------|-------------|
| LoginPage.razor.cs | LoginAsync | `_isSubmitting` ✓ | Add MudButton Disabled + LoadingIcon |
| RegisterPage.razor.cs | RegisterAsync | `_isSubmitting` ✓ | Add MudButton Disabled + LoadingIcon |
| ResetPasswordPage.razor.cs | ResetPasswordAsync | `_isSubmitting` ✓ | Add MudButton Disabled + LoadingIcon |
| UpdatePasswordPage.razor.cs | UpdatePasswordAsync | `_isSubmitting` ✓ | Add MudButton Disabled + LoadingIcon |
| UpdatePasswordPage.razor.cs | LogoutAsync | None | Add MudButton Disabled + LoadingIcon |

**Implementation:** Utilize existing `_isSubmitting` flag, add to button: `Disabled="@_isSubmitting"` and loading icon.

---

### B. MAIN PAGES (4 files)

**Priority: HIGH** - These are primary user entry points

#### 1. Home.razor.cs
- **Operations:** GetWorkoutPlansAsync (✓ has loading), CreateWorkoutPlanAsync
- **Current:** `_plans is null` check shows MudProgressCircular
- **Add:** Loading indicator on create button
- **Add:** Pagination loading on PageChanged event
- **Recommendation:** Replace null-check with `_isLoading` flag for better control

#### 2. PlanDetailPage.razor.cs
- **Operations:** 6 async operations (GetPlan, UpdatePlan, DeletePlan, AddExercise, RemoveExercise, StartSession)
- **Add:** MudProgressCircular on initial page load (`OnInitializedAsync`)
- **Add:** MudButton with disabled state for Save button
- **Add:** MudButton with disabled state for Delete button (confirm dialog)
- **Add:** MudButton with disabled state for Add Exercise button
- **Add:** MudIconButton with loading on remove exercise (per row)
- **Add:** MudButton with loading on Start Session button
- **Recommendation:** Create reusable `LoadingButtonState` helper

#### 3. WorkoutSessionPage.razor.cs
- **Operations:** 7 async operations during session workflow
- **Current:** Has MudProgressCircular on initial load
- **Add:** MudProgressLinear for exercise-by-exercise loading
- **Add:** Disabled state on Next button during exercise save
- **Add:** Disabled state on Cancel button during finish
- **Recommendation:** Show exercise count progress (e.g., "3/6 exercises completed")

#### 4. HistoryPage.razor.cs
- **Operations:** GetSessionHistoryAsync + pagination
- **Current:** Has MudProgressCircular on load
- **Add:** MudProgressLinear on pagination
- **Add:** Skeleton loaders for session cards
- **Recommendation:** Add date-based filtering with loading

---

### C. DIALOGS (7 files)

**Priority: HIGH** - Dialog submissions cause confusion without feedback

| Dialog | Operations | Recommended |
|--------|-----------|-------------|
| **CreatePlanDialog** | CreateWorkoutPlanAsync | MudButton Disabled + Icon during submit |
| **CreateExerciseDialog** | CreateExerciseAsync + GetMuscleGroupsAsync | MudOverlay + MudProgressCircular on load; Disabled button on submit |
| **ExercisePickerDialog** | GetExercisesAsync + SearchAsync + Pagination | MudProgressLinear on search; Loading skeleton on results |
| **ConfirmationDialog** | (Generic - no async) | Add Disabled state for action button |
| **PasswordConfirmationDialog** | (Generic - no async) | Add Disabled state for action button |
| **ActiveSessionDialog** | (Modal presentation only) | Add Disabled state for action buttons |

**Implementation Pattern for Dialogs:**
```csharp
private bool _isSubmitting = false;

private async Task Submit()
{
    _isSubmitting = true;
    try
    {
        await Service.CreateAsync(model);
        MudDialog.Close(DialogResult.Ok(result));
    }
    catch (Exception ex)
    {
        Snackbar.Add(ex.Message, Severity.Error);
    }
    finally
    {
        _isSubmitting = false;
    }
}
```

---

## PART 4: Recommended UI Patterns

### Pattern 1: Button with Loading State
```razor
<MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="@SaveAsync" Disabled="@_isSaving">
    @if (_isSaving)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
    }
    Save
</MudButton>
```

### Pattern 2: Page Loading with Overlay
```razor
@if (_isLoading)
{
    <MudOverlay Visible="true" DarkBackground="true">
        <MudProgressCircular Color="Color.Default" Indeterminate="true" />
    </MudOverlay>
}
```

### Pattern 3: Skeleton Loader for Cards
```razor
@if (_isLoading)
{
    <MudCard>
        <MudCardContent>
            <MudSkeleton Width="70%" />
            <MudSkeleton Width="80%" />
        </MudCardContent>
    </MudCard>
}
```

### Pattern 4: Progress for Multi-Step Operations
```razor
<MudProgressLinear Value="@(_currentExercise / _totalExercises * 100)" Color="Color.Info" />
<MudText Typo="Typo.caption">@_currentExercise/@_totalExercises exercises</MudText>
```

---

## PART 5: Implementation Priority & Effort Matrix

| Priority | File | Operations | Effort | Impact |
|----------|------|-----------|--------|--------|
| **CRITICAL** | LoginPage, RegisterPage, UpdatePasswordPage | Auth operations | Low | High - Core user journey |
| **CRITICAL** | Home.razor | Plan list loading + create | Low | High - Main dashboard |
| **HIGH** | PlanDetailPage | 6 operations | Medium | High - Frequent operations |
| **HIGH** | All Dialogs (7) | Form submissions | Low | Medium - Better UX |
| **HIGH** | WorkoutSessionPage | 7 operations | Medium | High - Active user operation |
| **MEDIUM** | HistoryPage | List + pagination | Low | Medium - Secondary feature |
| **MEDIUM** | SessionSummaryPage | Detail loading | Low | Low - View-only page |
| **MEDIUM** | SettingsPage | Delete account | Low | Low - Infrequent |

---

## PART 6: Implementation Checklist

### Phase 1: Core Infrastructure (Day 1)
- [ ] Create `LoadingState.cs` service for centralized state management
- [ ] Create reusable `LoadingButton.razor` component
- [ ] Add `_isLoading` pattern to all page code-behind files
- [ ] Update Program.cs DI if creating new service

### Phase 2: Authentication Pages (Day 1-2)
- [ ] LoginPage: Add button disabled state + loading icon
- [ ] RegisterPage: Add button disabled state + loading icon
- [ ] ResetPasswordPage: Add button disabled state + loading icon
- [ ] UpdatePasswordPage: Add button disabled state + loading icon + logout button

### Phase 3: Main Dashboard & Plans (Day 2-3)
- [ ] Home.razor: Convert null-check to `_isLoading`; add skeleton loaders
- [ ] PlanDetailPage: Add loading on all 6 async operations
- [ ] CreatePlanDialog: Add loading state to submit button

### Phase 4: Dialogs (Day 3)
- [ ] CreateExerciseDialog: Add overlay + loading on open; button loading on submit
- [ ] ExercisePickerDialog: Add loading on search; skeleton for results
- [ ] All other dialogs: Add disabled states

### Phase 5: Session Management (Day 4)
- [ ] WorkoutSessionPage: Add progress bar for exercises; loading on operations
- [ ] SessionSummaryPage: Add loading indicator

### Phase 6: Testing & Refinement (Day 5)
- [ ] Test slow network scenarios
- [ ] Verify no double-submit issues
- [ ] Check accessibility (ARIA labels)
- [ ] Performance testing

---

## PART 7: Code Structure Recommendation

### Create `LoadingState.cs` Helper

```csharp
namespace WorkoutManager.Web.Services;

public class LoadingState
{
    public bool IsLoading { get; set; }
    public string? LoadingMessage { get; set; }

    public void Start(string message = "Loading...")
    {
        IsLoading = true;
        LoadingMessage = message;
    }

    public void Stop()
    {
        IsLoading = false;
        LoadingMessage = null;
    }
}
```

### Create `LoadingButton.razor` Component

```razor
@using MudBlazor

<MudButton @attributes="@InputAttributes" Disabled="@(Disabled || IsLoading)" OnClick="@OnClick">
    @if (IsLoading)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
    }
    @ChildContent
</MudButton>

@code {
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? InputAttributes { get; set; }
}
```

---

## PART 8: Per-File Action Items

### Files Requiring Updates (20+ files)

**AUTHENTICATION (5 files)**
```
✓ LoginPage.razor.cs - Add button loading
✓ RegisterPage.razor.cs - Add button loading
✓ ResetPasswordPage.razor.cs - Add button loading
✓ UpdatePasswordPage.razor.cs - Add button loading (2 methods)
✓ Callback.razor - Add redirect loading indicator
```

**PAGES (6 files)**
```
✓ Home.razor.cs - Convert to _isLoading pattern
✓ PlanDetailPage.razor.cs - Add on 6 async operations
✓ WorkoutSessionPage.razor.cs - Add progress bar + button loading
✓ HistoryPage.razor.cs - Add pagination loading
✓ SessionSummaryPage.razor.cs - Add initial load indicator
✓ SettingsPage.razor.cs - Add delete operation loading
```

**DIALOGS (7 files)**
```
✓ CreatePlanDialog.razor.cs - Add button loading
✓ CreateExerciseDialog.razor.cs - Add overlay + button loading
✓ ExercisePickerDialog.razor.cs - Add search/pagination loading
✓ ConfirmationDialog.razor.cs - Add button disabled
✓ PasswordConfirmationDialog.razor.cs - Add button disabled
✓ ActiveSessionDialog.razor - Add button disabled
```

---

## PART 9: Implementation Examples

### Example 1: Simple Button Loading (LoginPage)

**Code-behind (LoginPage.razor.cs):**
```csharp
private bool _isSubmitting = false;

private async Task Submit()
{
    _isSubmitting = true;
    try
    {
        await AuthService.LoginAsync(Email, Password);
        NavigationManager.NavigateTo("/");
    }
    catch (Exception ex)
    {
        _errorMessage = ex.Message;
    }
    finally
    {
        _isSubmitting = false;
    }
}
```

**Razor Template (LoginPage.razor):**
```razor
<MudButton Variant="Variant.Filled" Color="Color.Primary" OnClick="@Submit" Disabled="@_isSubmitting">
    @if (_isSubmitting)
    {
        <MudIcon Class="mr-2">@Icons.Material.Filled.CloudUpload</MudIcon>
    }
    Sign In
</MudButton>
```

### Example 2: Page Loading with Overlay (Home)

**Code-behind (Home.razor.cs):**
```csharp
private bool _isLoading = false;
private IEnumerable<WorkoutPlanDto> _plans = new();

protected override async Task OnInitializedAsync()
{
    _isLoading = true;
    try
    {
        var result = await WorkoutPlanService.GetWorkoutPlansAsync(
            pageNumber: PageNumber,
            pageSize: PageSize);
        _plans = result.Items;
        _pagination = result.PaginationInfo;
    }
    finally
    {
        _isLoading = false;
    }
}
```

**Razor Template (Home.razor):**
```razor
@if (_isLoading)
{
    <MudOverlay Visible="true" DarkBackground="true">
        <MudProgressCircular Color="Color.Default" Indeterminate="true" />
    </MudOverlay>
}
else
{
    @foreach (var plan in _plans)
    {
        <!-- Plan card content -->
    }
}
```

### Example 3: Progress Bar for Multi-Step (WorkoutSessionPage)

**Code-behind (WorkoutSessionPage.razor.cs):**
```csharp
private int _currentExerciseIndex = 0;
private int _totalExercises = 0;
private bool _isLoadingExercise = false;

protected override async Task OnInitializedAsync()
{
    _session = await SessionService.GetSessionDetailsAsync(SessionId);
    _totalExercises = _session.Exercises.Count;
}

private async Task NextExercise()
{
    _isLoadingExercise = true;
    try
    {
        await UpdateCurrentExercise();
        _currentExerciseIndex++;
    }
    finally
    {
        _isLoadingExercise = false;
    }
}
```

**Razor Template (WorkoutSessionPage.razor):**
```razor
<MudProgressLinear Value="@(_currentExerciseIndex / (double)_totalExercises * 100)" Color="Color.Info" />
<MudText Typo="Typo.caption">Exercise @(_currentExerciseIndex + 1)/@_totalExercises</MudText>

<MudButton OnClick="@NextExercise" Disabled="@_isLoadingExercise">
    @if (_isLoadingExercise)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
    }
    Next Exercise
</MudButton>
```

---

## PART 10: Async Operations Summary

### Total Async Operations by Category

| Category | Count | Files | Operations |
|----------|-------|-------|-----------|
| Authentication | 6 | 5 pages | Login, Register, ResetPassword, UpdatePassword, Logout, Callback |
| Page Loading | 4 | 6 pages | Home, PlanDetail, WorkoutSession, History, SessionSummary, Settings |
| Plan Management | 8 | 1 dialog | Get, Create, Update, Delete, AddExercise, RemoveExercise, Reorder |
| Exercise Management | 4 | 2 dialogs | GetExercises, CreateExercise, GetPrevious, GetById |
| Session Management | 7 | 1 page | GetActive, GetDetails, GetHistory, Start, Update, UpdateExercise, Finish |
| Dialog Operations | 7 | 7 dialogs | Search, Filter, Pagination, Submit operations |

**Total: 21+ async operations across 20+ files**

---

## Summary & Next Steps

| Aspect | Status | Notes |
|--------|--------|-------|
| **Current Loading Indicators** | 3/20 pages | Only Home, History, WorkoutSession |
| **Button Loading States** | None | Authentication pages need them most |
| **Dialog Feedback** | None | 7 dialogs without submission feedback |
| **Skeleton Loaders** | None | MudBlazor available but not used |
| **Error Handling** | Partial | Snackbar exists, but no retry mechanisms |

### Recommended Execution Timeline

| Phase | Duration | Files | Impact |
|-------|----------|-------|--------|
| **Phase 1: Infrastructure** | 1 day | Core services | Foundation |
| **Phase 2: Authentication** | 1 day | 5 pages | High - Core user journey |
| **Phase 3: Main Features** | 2 days | Home, PlanDetail | High - Frequent use |
| **Phase 4: Dialogs** | 1 day | 7 components | Medium - UX improvement |
| **Phase 5: Advanced Features** | 1 day | Pagination, Progress | Medium - Polish |
| **Phase 6: Testing** | 1 day | All areas | Validation |

**Total Estimated Effort: 5-7 days of implementation**

---

## References

- MudBlazor Documentation: https://mudblazor.com/
- Tech Stack: See @.ai/tech-stack.md
- Related Project Plans:
  - @.ai/ui-plan.md
  - @.ai/implementation-summary.md
  - @.ai/test-plan.md
