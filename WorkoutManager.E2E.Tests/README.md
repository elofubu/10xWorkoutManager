# E2E Tests - Workout Manager

## Overview
This project contains End-to-End (E2E) tests for the Workout Manager application, implemented using Microsoft Playwright for .NET, xUnit, and FluentAssertions.

## Test Structure

### Test Categories

#### 1. Authentication Tests (`AuthenticationTests.cs`)
Tests covering user authentication flows:
- ✅ `LoginPage_ShouldBeAccessible` - Verify login page loads
- ✅ `Login_WithValidCredentials_ShouldSucceed` - Successful login flow
- ✅ `Login_WithInvalidCredentials_ShouldShowError` - Failed login with error message
- ✅ `Logout_ShouldRedirectToLoginPage` - Logout functionality
- ✅ `ProtectedRoute_WithoutAuthentication_ShouldRedirectToLogin` - Unauthenticated route protection
- ✅ `Login_ShouldPersistSession_InLocalStorage` - Session persistence verification
- ✅ `Logout_ShouldClearSession_FromLocalStorage` - Session cleanup on logout

#### 2. Security Tests (`SecurityTests.cs`)
Tests verifying security controls:
- ✅ `ProtectedRoute_WithoutAuth_ShouldRedirectToLogin` (Theory: /, /history, /settings)
- ✅ `AuthenticatedUser_CanAccessProtectedRoutes` - Authenticated access verification
- ⏭️ `UserIsolation_User1_CannotAccessUser2Data` - Cross-user data isolation (Skipped - requires test data)
- ✅ `Session_AfterLogout_ShouldNotPersist` - Session invalidation
- ✅ `DirectAPIAccess_WithoutToken_ShouldBeBlocked` - API authentication verification
- ✅ `LocalStorageSession_ShouldBeHttpOnly` - Session storage security

#### 3. Session Management Tests (`SessionManagementTests.cs`)
Tests for workout session management:
- ⏭️ `StartWorkout_WithActiveSession_ShouldShowDialog` - Prevent multiple active sessions (Skipped - requires setup)
- ⏭️ `ResumeSession_ShouldContinueFromWhereLeft` - Session resume functionality (Skipped - requires dialog helpers)
- ⏭️ `CancelWorkout_ShouldNotSaveSession` - Cancel workout flow (Skipped - requires API integration)
- ✅ `Navigation_BetweenPages_ShouldMaintainAuthentication` - Auth persistence across navigation

#### 4. User Journey Tests (`UserJourneyTests.cs`)
End-to-end user workflow tests:
- ✅ `FullUserJourney_CreatePlan_StartSession_CompleteWorkout_VerifyHistory` - Core workflow verification
- ⏭️ `CompleteWorkoutFlow_WithExistingPlan` - Complete workout with data (Skipped - requires dialog helpers)

## Page Objects

### Implemented Page Objects
Located in `PageObjects/`:

1. **BasePage.cs** - Base class with common page operations
   - Navigation helpers
   - Element interaction methods (Click, Fill, GetText)
   - Element visibility checks

2. **LoginPage.cs** - Login page interactions
   - `NavigateAsync()` - Navigate to login page
   - `LoginAsync(email, password)` - Perform login
   - `LogoutAsync()` - Perform logout
   - `IsLoginFormVisibleAsync()` - Check form visibility
   - `HasErrorMessageAsync()` / `GetErrorMessageAsync()` - Error handling

3. **HomePage.cs** - Home/Plans page interactions
   - `NavigateAsync()` - Navigate to home
   - `IsAuthenticatedAsync()` - Check authentication state
   - `GetPlanCountAsync()` - Count visible plans
   - `PlanExistsByNameAsync(name)` - Find plan by name
   - `ClickCreatePlanButtonAsync()` - Trigger plan creation

4. **PlanDetailPage.cs** - Plan details page interactions
   - `NavigateAsync(planId)` - Navigate to specific plan
   - `GetPlanNameAsync()` - Get plan name
   - `HasTrainingDaysAsync()` - Check for training days
   - `ClickStartWorkoutForFirstDayAsync()` - Start workout
   - `EnableEditModeAsync()` - Toggle edit mode

5. **WorkoutSessionPage.cs** - Workout session page interactions
   - `NavigateAsync(sessionId)` - Navigate to session
   - `FillSetDataAsync(index, weight, reps, toFailure)` - Fill exercise data
   - `ClickAddSetAsync()` - Add new set
   - `ClickSaveAndNextAsync()` - Move to next exercise
   - `ClickFinishWorkoutAsync()` - Complete workout
   - `FillSessionNotesAsync(notes)` - Add session notes

6. **HistoryPage.cs** - History page interactions
   - `NavigateAsync()` - Navigate to history
   - `GetSessionCountAsync()` - Count completed sessions
   - `SessionExistsWithPlanNameAsync(planName)` - Find session by plan name
   - `ClickFirstSessionAsync()` - Open session details

## Test Data

### TestData Helper (`Helpers/TestData.cs`)
Centralized test data management:

```csharp
TestData.Users.User1.Email        // user1@test.local
TestData.Users.User1.Password     // Test123!@#
TestData.Users.User2.Email        // user2@test.local
TestData.Users.InvalidUser.Email  // invalid@test.local

TestData.Routes.Home              // /
TestData.Routes.Login             // /authentication/login
TestData.Routes.History           // /history
```

## Running Tests

### Prerequisites
1. **Test Users**: Ensure test users exist in Supabase:
   - `user1@test.local` / `Test123!@#`
   - `user2@test.local` / `Test123!@#`

2. **Running Applications**:
   - API: `https://localhost:5048`
   - Web: `http://localhost:5000`
   - Supabase: `http://127.0.0.1:54321`

### Run All Tests
```bash
cd WorkoutManager.E2E.Tests
dotnet test
```

### Run Specific Test Category
```bash
# Authentication tests only
dotnet test --filter "FullyQualifiedName~AuthenticationTests"

# Security tests only
dotnet test --filter "FullyQualifiedName~SecurityTests"
```

### Run Tests with Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Tests in Headful Mode (for debugging)
Modify `BasePlaywrightTest.cs`:
```csharp
Headless = false  // Line 20
```

## Test Execution Summary

### ✅ Ready to Run (13 tests)
- All authentication tests (7 tests)
- Most security tests (5 tests)
- Navigation maintenance test (1 test)

### ⏭️ Skipped Tests (8 tests)
These tests require additional setup or implementation:
- Dialog/modal interaction helpers
- Test data seeding (plans, exercises)
- Session state verification helpers

## Coverage Status

### Implemented
✅ User authentication (login, logout, session management)
✅ Protected route verification
✅ Session persistence in localStorage
✅ API authentication requirements
✅ Basic navigation flows

### Partially Implemented
⚠️ Full user journey (structure ready, needs dialog interaction)
⚠️ Session management (structure ready, needs active session handling)

### Not Yet Implemented
❌ Plan creation and modification dialogs
❌ Exercise selection and management
❌ Complete workout flow with data entry
❌ User isolation testing (requires multi-user test data)

## Next Steps

### Immediate Priorities
1. **Test Data Setup**:
   - Create seeding script for test users
   - Add sample plans and exercises for testing
   - Document test data requirements

2. **Dialog Helpers**:
   - Implement modal/dialog interaction utilities
   - Add helpers for CreatePlanDialog, ExercisePickerDialog
   - Handle confirmation dialogs

3. **CI/CD Integration**:
   - Add GitHub Actions workflow
   - Configure headless browser execution
   - Set up test result reporting

### Future Enhancements
- Snapshot testing for API contracts (Verify.Http)
- Performance benchmarks (response times)
- Accessibility testing (WCAG compliance)
- Visual regression testing
- Multi-browser testing (Firefox, WebKit)

## Browser Configuration

Current configuration (`BasePlaywrightTest.cs`):
- Browser: Chromium
- Mode: Headless
- Viewport: 1280x720 (inherited from base)

## Known Issues & Limitations

1. **Test Users**: Tests assume users exist in Supabase. Manual creation required.
2. **Dialog Interactions**: MudBlazor dialogs require specialized selectors and timing.
3. **Base URL**: Currently hardcoded to `http://localhost:5000` - should be configurable.
4. **API URL**: Assumes API runs on port 5048 - should be configurable.
5. **Test Isolation**: Tests don't clean up created data - may cause pollution.

## Configuration Recommendations

### Environment Variables
Consider adding:
```bash
WEB_URL=http://localhost:5000
API_URL=https://localhost:5048
SUPABASE_URL=http://127.0.0.1:54321
TEST_USER_EMAIL=user1@test.local
TEST_USER_PASSWORD=Test123!@#
```

### Test Data Cleanup
Implement `IAsyncLifetime.DisposeAsync` in test classes to:
- Delete created plans
- Cancel active sessions
- Reset test user state

## Contributing

When adding new tests:
1. Follow Page Object Model pattern
2. Use descriptive test names with Given_When_Then format
3. Add appropriate `[Fact]` or `[Theory]` attributes
4. Use `[Skip("reason")]` for tests requiring manual setup
5. Include helpful assertion messages with `.Should().Be*("because...")`

## Resources

- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Test Plan](.ai/test-plan.md)
- [Tech Stack](.ai/tech-stack.md)
