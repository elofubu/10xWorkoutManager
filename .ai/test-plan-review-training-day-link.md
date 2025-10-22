# Test Plan Review: Training Day Link to Sessions

## Executive Summary

The addition of `training_day_id` to the `sessions` table requires **3 new test scenarios** to ensure complete coverage. The existing test infrastructure is fully compatible. **Every session must be linked to a training day** - this is a required field.

---

## 1. Impact Analysis

### ✅ What Stays Compatible

The following test scenarios from `test-plan.md` **remain valid** without changes:

- **Section 4.1**: Authentication and authorization tests - ✅ No impact
- **Section 4.2**: Workout plan management - ✅ No impact
- **Section 4.4**: Exercise and performance tests - ✅ No impact
- **Section 4.5**: Validator tests - ✅ No impact  
- **Section 4.6**: Error handling - ✅ No impact
- **Existing session tests** in `SessionsControllerTests.cs` - ✅ All pass (they already use `TrainingDayId`)

### ✅ No Breaking Changes

**Current Code** is compatible:
```csharp
public class StartSessionCommand
{
    public int TrainingDayId { get; set; }  // Required
}
```

**Service Signature**:
```csharp
Task<SessionDetailsDto> StartSessionAsync(long trainingDayId, Guid userId);
```

**Database**:
```sql
ALTER TABLE public.sessions 
ADD COLUMN training_day_id BIGINT NOT NULL
REFERENCES public.training_days(id) ON DELETE RESTRICT;
```

---

## 2. Missing Test Coverage

### 2.1 Session Start Tests (Section 4.3)

**Existing Coverage** (from `SessionsControllerTests.cs`):
- ✅ Start session with valid training day
- ✅ Start session returns 404 when training day doesn't exist
- ✅ Start session returns 409 when user has active session
- ✅ Start session returns 404 when training day ID is 0

**Missing Coverage**:

#### Test Case 1: Training Day Name in Response
```csharp
[Fact]
public async Task StartSession_Should_Return_TrainingDayName_When_Started_From_TrainingDay()
{
    // Arrange
    Authenticate();
    var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();
    
    var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };

    // Act
    var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var result = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
    result.Should().NotBeNull();
    result.TrainingDayId.Should().Be(trainingDayId);
    result.TrainingDayName.Should().NotBeNullOrEmpty();
    result.PlanName.Should().NotBeNullOrEmpty();
    result.PlanId.Should().Be(plan.Id);
}
```

**Test Plan Section**: 4.3 - Start sesji

**Priority**: P1 (High) - Core functionality for UI display

---

### 2.2 Session History Tests (Section 4.3)

**Existing Coverage**:
- ✅ Get sessions with pagination
- ✅ Get only user-owned sessions

**Missing Coverage**:

#### Test Case 2: Training Day Info in Session History
```csharp
[Fact]
public async Task GetSessions_Should_Return_TrainingDayName_For_Each_Session()
{
    // Arrange
    Authenticate();
    var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

    // Create session from training day
    var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
    var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", command);
    var session = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
    
    // End session
    var endCommand = new UpdateSessionCommand { EndTime = DateTime.UtcNow };
    await HttpClient.PutAsJsonAsync($"/api/sessions/{session!.Id}", endCommand);

    // Act
    var response = await HttpClient.GetAsync("/api/sessions");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
    result.Should().NotBeNull();
    result.Data.Should().HaveCount(1);
    result.Data[0].TrainingDayId.Should().Be(trainingDayId);
    result.Data[0].TrainingDayName.Should().NotBeNullOrEmpty();
    result.Data[0].PlanName.Should().NotBeNullOrEmpty();
}
```

**Test Plan Section**: 4.3 - Historia sesji

**Priority**: P1 (High) - Core feature for tracking which day was performed

---

### 2.3 E2E Tests (Section 4.7)

**Existing Coverage**:
- ✅ Full flow: Login → Create plan → Start session → Finish → Verify history

**Missing Coverage**:

#### Test Case 3: E2E - Training Day Display in UI
```javascript
// Playwright test
test('should display training day name in session history', async ({ page }) => {
  await loginAsTestUser(page);
  
  // Create plan with training day
  await page.goto('/plans/new');
  await page.fill('input[name="planName"]', 'Test Plan');
  await page.click('button:has-text("Save")');
  
  // Add training day
  await page.click('button:has-text("Add Day")');
  await page.fill('input[name="dayName"]', 'Push Day');
  await page.click('button:has-text("Save Day")');
  
  // Start session from this training day
  await page.click('button:has-text("Start Workout")');
  await page.click('text=Push Day'); // Select training day
  await page.click('button:has-text("Start")');
  
  // Finish session
  await page.click('button:has-text("Finish Workout")');
  
  // Verify in history
  await page.goto('/sessions/history');
  await expect(page.locator('text=Push Day')).toBeVisible();
  await expect(page.locator('text=Test Plan')).toBeVisible();
});
```

**Test Plan Section**: 4.7 - Scenariusze E2E (UI)

**Priority**: P1 (High) - Critical user journey

---

## 3. Test Plan Updates Required

### Section 4.3 - Update "Sesje treningowe"

**Current Text**:
> - **Start sesji**:
>   - Bez aktywnej sesji -> 201 z utworzonymi `SessionExercise` zgodnie z kolejnością dnia.

**Updated Text**:
> - **Start sesji**:
>   - **Wymagane `training_day_id`**: Każda sesja musi być powiązana z dniem treningowym.
>   - Bez aktywnej sesji -> 201 z utworzonymi `SessionExercise` zgodnie z kolejnością dnia.
>   - Odpowiedź zawiera `training_day_id`, `training_day_name`, `plan_id`, `plan_name`.
>   - Brak `training_day_id` w żądaniu -> 400 (walidacja).

**Add New Item**:
> - **Historia sesji**:
>   - Każda sesja wyświetla `training_day_name` i `plan_name`.
>   - Dane są zawsze wypełnione (każda sesja ma przypisany dzień treningowy).

### Section 4.8 - NEW SECTION: Data Integrity Tests

**Add After Section 4.7**:
```markdown
### 4.8 Testy integralności danych

- **Kaskadowe usunięcia i relacje**:
  - Próba usunięcia training day z sesjami -> BŁĄD (ON DELETE RESTRICT).
  - Użytkownik musi najpierw usunąć sesje, aby usunąć training day.
  - Usunięcie planu -> plan_id w sesjach ustawione na NULL (ON DELETE SET NULL).
  - Usunięcie użytkownika -> wszystkie sesje usunięte (ON DELETE CASCADE).
- **Wymagane pole training_day_id**:
  - Baza danych wymusza NOT NULL na training_day_id.
  - API waliduje obecność TrainingDayId w StartSessionCommand.
  - Testy weryfikują, że sesje nie mogą być utworzone bez training day.
```

---

## 4. Summary Table

| Test Case | Priority | Type | Status | Section in Test Plan |
|-----------|----------|------|--------|---------------------|
| Training day name in response | P1 | Integration | ❌ Missing | 4.3 |
| Training day info in history | P1 | Integration | ❌ Missing | 4.3 |
| E2E - Training day display in UI | P1 | E2E | ❌ Missing | 4.7 |

**Total New Tests**: 3  
**Breaking Changes**: 0  
**Test Plan Sections to Update**: 2 (4.3, 4.8-new)

---

## 5. Implementation Checklist

### Code Changes (✅ Complete)
- [x] `StartSessionCommand.TrainingDayId` is required (int, not nullable)
- [x] `SessionsController` handles required `TrainingDayId`
- [x] Migration makes `training_day_id` NOT NULL
- [x] Service layer simplified (no nullable handling)

### Test Implementation (Tydzień 2 - Integration Tests)
- [ ] Test: Training day name in session response
- [ ] Test: Training day info in session history

### E2E Tests (Tydzień 3)
- [ ] E2E: Training day display in session history UI

### Documentation
- [ ] Update test-plan.md Section 4.3
- [ ] Add test-plan.md Section 4.8 (Data Integrity)

---

## 6. Risk Assessment

### Low Risk ✅
- All existing tests remain valid
- Database migration is straightforward (NOT NULL on new column)
- No changes to authentication/authorization logic
- Simple, clean implementation

### What to Watch ⚠️
- UI must always require training day selection
- Users cannot delete training days that have associated sessions (ON DELETE RESTRICT)
- Need clear error messages when constraint violations occur

### Mitigation
- ✅ Required field enforced at DB, service, and command level
- ✅ Clear validation messages
- ✅ Comprehensive tests before deploying

---

## 7. Next Steps

1. **Week 2** (Integration Tests):
   - Implement 2 integration tests
   - Update test-plan.md
   
2. **Week 3** (E2E):
   - Implement 1 E2E test for UI display
   
3. **Verification**:
   - Run full test suite
   - Verify 100% pass rate
   - Update coverage metrics

---

## Conclusion

The `training_day_id` addition is **simple and clean**. With the required field design and 3 new test cases, the feature will be fully covered and production-ready.

**Recommendation**: ✅ Proceed with implementation - no blockers.
