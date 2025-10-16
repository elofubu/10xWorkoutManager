# Integration Tests Coverage Plan

## Current Coverage Status

### Controllers with Tests ✅
- **ExercisesControllerTests.cs** - Covers ExercisesController
- **WorkoutPlansControllerTests.cs** - Covers WorkoutPlansController

### Controllers Missing Tests ❌
- MuscleGroupsController
- SessionsController
- SessionExercisesController
- PlanDayExercisesController

---

## Authorization Status

### Controllers with [Authorize] Attribute
- ✅ SessionsController - **All endpoints require authentication**
- ✅ SessionExercisesController - **All endpoints require authentication**
- ✅ PlanDayExercisesController - **All endpoints require authentication**
- ✅ MuscleGroupsController - **All endpoints require authentication**
- ✅ ExercisesController - **All endpoints require authentication**

---

## Detailed Implementation Plan

### 1. MuscleGroupsController Tests
**File:** `WorkoutManager.Api.Tests/Controllers/MuscleGroupsControllerTests.cs`

**Endpoints to Test:**
- `GET /api/musclegroups` - GetMuscleGroups
- `GET /api/musclegroups/{id}` - GetMuscleGroupById

**Test Cases:**
```
✓ Should return OK with all muscle groups
✓ Should return muscle group by ID when it exists
✓ Should return NotFound when muscle group ID doesn't exist
✓ Should handle exceptions gracefully (500 status)
✓ No authentication required - public endpoint
```

**Special Considerations:**
- MuscleGroups are shared data (not user-specific)
- No authorization required
- No database setup needed (muscle groups are pre-populated)
- No `Authenticate()` call needed in tests

---

### 2. SessionsController Tests
**File:** `WorkoutManager.Api.Tests/Controllers/SessionsControllerTests.cs`

**Authorization:** `[Authorize]` attribute added ✅

**Endpoints to Test:**
- `GET /api/sessions` - GetSessions (paginated)
- `GET /api/sessions/{id}` - GetSessionById
- `GET /api/sessions/active` - GetActiveSession
- `POST /api/sessions` - StartSession
- `PUT /api/sessions/{id}` - UpdateSession

**Test Cases:**

**Authentication & Authorization (Required for ALL endpoints):**
```
✓ Should return Unauthorized (401) when no token provided
✓ Should return Forbidden (403) when accessing other user's sessions
✓ Should return OK when token is valid
```

**GET /api/sessions (List):**
```
✓ Should return Unauthorized without authentication
✓ Should return paginated sessions for authenticated user
✓ Should return only user-owned sessions
✓ Should support pagination (page, pageSize parameters)
✓ Should handle empty results gracefully
```

**GET /api/sessions/{id}:**
```
✓ Should return Unauthorized without authentication
✓ Should return session details when user owns session
✓ Should return NotFound when session doesn't exist
✓ Should return Forbidden when user doesn't own session
```

**GET /api/sessions/active:**
```
✓ Should return Unauthorized without authentication
✓ Should return active session for user
✓ Should return NoContent when no active session exists
```

**POST /api/sessions (Start):**
```
✓ Should return Unauthorized without authentication
✓ Should create new session with valid StartSessionCommand
✓ Should return 201 Created with session details
✓ Should return NotFound if TrainingDayId doesn't exist
✓ Should return Conflict if business rule violated (e.g., already active)
✓ Should return BadRequest if validation fails
```

**PUT /api/sessions/{id} (Update):**
```
✓ Should return Unauthorized without authentication
✓ Should update session notes
✓ Should finish session when EndTime is provided
✓ Should return NoContent on success
✓ Should return NotFound when session doesn't exist
✓ Should return Conflict on business rule violation
✓ Should return Forbidden when user doesn't own session
```

---

### 3. SessionExercisesController Tests
**File:** `WorkoutManager.Api.Tests/Controllers/SessionExercisesControllerTests.cs`

**Authorization:** `[Authorize]` attribute added ✅

**Endpoints to Test:**
- `PUT /api/sessions/{sessionId}/exercises/{sessionExerciseId}` - UpdateSessionExercise

**Test Cases:**
```
✓ Should return Unauthorized without authentication
✓ Should update session exercise with valid data
✓ Should return updated exercise details
✓ Should return NotFound when session or exercise doesn't exist
✓ Should return BadRequest on validation failure
✓ Should return Forbidden when user doesn't own session
```

**Special Considerations:**
- All endpoints require authentication (new: [Authorize] added)
- Requires existing session data (create test session first)
- Requires existing session exercises (add exercises to session first)
- Validates that user owns the session

---

### 4. PlanDayExercisesController Tests
**File:** `WorkoutManager.Api.Tests/Controllers/PlanDayExercisesControllerTests.cs`

**Authorization:** `[Authorize]` attribute added ✅

**Endpoints to Test:**
- `POST /api/workout-plans/{planId}/training-days/{dayId}/exercises` - AddExerciseToTrainingDay
- `DELETE /api/workout-plans/{planId}/training-days/{dayId}/exercises/{planDayExerciseId}` - RemoveExerciseFromTrainingDay

**Test Cases:**

**POST (Add Exercise):**
```
✓ Should return Unauthorized without authentication
✓ Should add exercise to training day with valid command
✓ Should return 201 Created with exercise details
✓ Should return NotFound when plan or day doesn't exist
✓ Should return 403 Forbidden on business rule violation (user doesn't own plan)
✓ Should return BadRequest on validation failure
```

**DELETE (Remove Exercise):**
```
✓ Should return Unauthorized without authentication
✓ Should remove exercise from training day
✓ Should return NoContent on success
✓ Should return NotFound when exercise doesn't exist
✓ Should return 403 Forbidden when user doesn't own plan
```

**Special Considerations:**
- All endpoints require authentication (new: [Authorize] added)
- Requires existing workout plan data
- Requires existing training day
- Requires existing exercises to add
- Validates user ownership of plan

---

## Implementation Priority

### Phase 1 (High Priority)
1. **MuscleGroupsController** - Simple, no auth, fast to implement
2. **SessionsController** - Core business logic, high priority, now requires auth

### Phase 2 (Medium Priority)
3. **SessionExercisesController** - Depends on Sessions working, now requires auth
4. **PlanDayExercisesController** - Depends on Plans and exercises, now requires auth

---

## Test Setup Requirements

### Database Fixtures Needed
- Create test sessions with proper states (active, completed)
- Create test training days
- Create test plan-day-exercise relationships
- Ensure muscle groups are available in test database

### Test Data Generator Enhancements
- Add SessionFaker for generating test sessions
- Add TrainingDayFaker for generating test days
- Add SessionExerciseFaker for generating session exercises

### Helper Methods in BaseIntegrationTest
- `CreateTestSession(userId, planId)` - Helper to create sessions
- `CreateTestPlanWithDays(userId)` - Helper to create plans with training days
- `CreateTestSessionExercise(sessionId, exerciseId)` - Helper for session exercises
- `Authenticate()` - Now required for all protected endpoints ✅ (already implemented)

---

## Estimated Effort

| Controller | Endpoints | Test Cases | Est. Hours | Auth Required |
|-----------|-----------|-----------|-----------|---------------|
| MuscleGroups | 2 | 4 | 1 | No |
| Sessions | 5 | 15 | 3.5 | Yes [Authorize] ✅ |
| SessionExercises | 1 | 6 | 1.5 | Yes [Authorize] ✅ |
| PlanDayExercises | 2 | 8 | 2 | Yes [Authorize] ✅ |
| **Total** | **10** | **33** | **8** | - |

---

## Key Changes from Previous Plan

### New Authorization Requirements
- ✅ SessionsController now has `[Authorize]` - all tests must call `Authenticate()`
- ✅ SessionExercisesController now has `[Authorize]` - all tests must call `Authenticate()`
- ✅ PlanDayExercisesController now has `[Authorize]` - all tests must call `Authenticate()`

### Test Count Increase
- Added 2 new test cases for authorization (now 33 vs 31)
- Estimated effort increased from 7.5 to 8 hours

### Authentication in Tests
- MuscleGroupsController tests: **No `Authenticate()` call needed**
- All other controller tests: **Must call `Authenticate()` before API calls**

---

## Success Criteria

- [ ] All 4 test classes created and implement required test cases
- [ ] All tests pass successfully
- [ ] Authentication tests verify 401 Unauthorized responses
- [ ] Authorization tests verify 403 Forbidden for unauthorized access
- [ ] Code coverage for all controller endpoints > 80%
- [ ] All edge cases and error scenarios covered
- [ ] Test data properly cleaned up between tests
- [ ] All protected endpoints tested both with and without authentication
