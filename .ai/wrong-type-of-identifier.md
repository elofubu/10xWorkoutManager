# Refactoring Plan: ID Type Inconsistency (int → long)

## Executive Summary

All model classes in `WorkoutManager.Data/Models/` use `long` for their ID properties, but most of the application layer (DTOs, Controllers, Services, Repositories) incorrectly uses `int` for ID parameters and properties. This creates:

1. **Type mismatch bugs** - Already identified in `PlanExerciseService.cs:30` where `int planId` is passed to methods expecting `long`
2. **Data truncation risk** - IDs larger than `int.MaxValue` (2,147,483,647) will cause runtime errors
3. **Architectural inconsistency** - Only WorkoutPlan-related code correctly uses `long`

## Current State Analysis

### ✅ Correct (Already using `long`)
- All Data Models (Exercise, WorkoutPlan, Session, etc.)
- WorkoutPlanDtos.cs (all DTOs)
- WorkoutPlansController (all methods)
- IWorkoutPlanService & WorkoutPlanService
- IWorkoutPlanRepository & WorkoutPlanRepository

### ❌ Incorrect (Using `int` instead of `long`)

#### 1. DTOs - 15 properties to change

**MuscleGroupDto.cs**
- `MuscleGroupDto.Id` (int → long)

**ExerciseDtos.cs**
- `CreateExerciseDto.MuscleGroupId` (int → long)
- `ExerciseDto.Id` (int → long)
- `ExerciseDto.MuscleGroupId` (int → long)

**SessionDtos.cs**
- `SessionSummaryDto.Id` (int → long)
- `SessionSummaryDto.PlanId` (int → long)
- `SessionDetailsDto.Id` (int → long)
- `SessionExerciseDetailsDto.Id` (int → long)
- `SessionExerciseDetailsDto.ExerciseId` (int → long)
- `ExerciseSetDto.Id` (int → long)

#### 2. Controllers - 13 methods to change

**PlanDayExercisesController.cs**
- `AddExerciseToTrainingDay(int planId, int dayId, ...)` → `(long planId, long dayId, ...)`
- `RemoveExerciseFromTrainingDay(int planId, int dayId, int planDayExerciseId, ...)` → `(long, long, long, ...)`

**SessionExercisesController.cs**
- `UpdateSessionExercise(int sessionId, int sessionExerciseId, ...)` → `(long, long, ...)`

**ExercisesController.cs**
- `GetExercises(int? muscleGroupId)` → `(long? muscleGroupId)`
- `GetExerciseById(int id)` → `(long id)`
- `GetPreviousPerformance(int exerciseId)` → `(long exerciseId)`

**SessionsController.cs**
- `GetSessionById(int id)` → `(long id)`
- `UpdateSession(int id, ...)` → `(long id, ...)`

**MuscleGroupsController.cs**
- `GetMuscleGroupById(int id)` → `(long id)`

#### 3. Service Interfaces - 18 methods to change

**IExerciseService.cs**
- `GetExercisesAsync(int? muscleGroupId, ...)` → `(long? muscleGroupId, ...)`
- `GetExerciseByIdAsync(int exerciseId)` → `(long exerciseId)`
- `GetLastPerformanceAsync(int exerciseId, ...)` → `(long exerciseId, ...)`

**IMuscleGroupService.cs**
- `GetMuscleGroupByIdAsync(int id)` → `(long id)`

**IPlanExerciseService.cs**
- `AddExerciseToDayAsync(int planId, int dayId, ...)` → `(long planId, long dayId, ...)`
- `RemoveExerciseFromDayAsync(int planId, int dayId, int planDayExerciseId, ...)` → `(long, long, long, ...)`
- `ReorderExercisesAsync(int planId, int dayId, ...)` → `(long planId, long dayId, ...)`

**ISessionService.cs**
- `StartSessionAsync(int trainingDayId, ...)` → `(long trainingDayId, ...)`
- `GetSessionByIdAsync(int sessionId, ...)` → `(long sessionId, ...)`
- `UpdateSessionNotesAsync(int sessionId, ...)` → `(long sessionId, ...)`
- `FinishSessionAsync(int sessionId, ...)` → `(long sessionId, ...)`

**ISessionExerciseService.cs**
- `UpdateSessionExerciseAsync(int sessionId, int sessionExerciseId, ...)` → `(long, long, ...)`
- `MarkAsSkippedAsync(int sessionExerciseId, ...)` → `(long sessionExerciseId, ...)`

#### 4. Repository Interfaces - 21 methods to change

**IExerciseRepository.cs**
- `GetExerciseByIdAsync(int exerciseId)` → `(long exerciseId)`
- `GetLastPerformanceAsync(int exerciseId, ...)` → `(long exerciseId, ...)`

**IMuscleGroupRepository.cs**
- `GetByIdAsync(int id)` → `(long id)`

**IPlanExerciseRepository.cs**
- `GetPlanByIdAndUserIdAsync(int planId, ...)` → `(long planId, ...)`
- `GetTrainingDayByIdAndPlanIdAsync(int dayId, int planId)` → `(long dayId, long planId)`
- `GetExerciseByIdAsync(int exerciseId)` → `(long exerciseId)`
- `GetPlanDayExerciseAsync(int planDayExerciseId, int dayId)` → `(long, long)`
- `RemoveExerciseFromDayAsync(int planDayExerciseId)` → `(long planDayExerciseId)`
- `ReorderExercisesAsync(int dayId, ...)` → `(long dayId, ...)`

**ISessionRepository.cs**
- `GetTrainingDayByIdAsync(int trainingDayId)` → `(long trainingDayId)`
- `GetPlanDayExercisesAsync(int trainingDayId)` → `(long trainingDayId)`
- `GetSessionByIdAsync(int sessionId, ...)` → `(long sessionId, ...)`
- `GetSessionExercisesWithSetsAsync(int sessionId)` → `(long sessionId)`

**ISessionExerciseRepository.cs**
- `GetSessionByIdAndUserIdAsync(int sessionId, ...)` → `(long sessionId, ...)`
- `GetSessionExerciseByIdAndSessionIdAsync(int sessionExerciseId, int sessionId)` → `(long, long)`
- `GetSessionExerciseWithSessionAsync(int sessionExerciseId, ...)` → `(long sessionExerciseId, ...)`
- `DeleteSetsForSessionExerciseAsync(int sessionExerciseId)` → `(long sessionExerciseId)`
- `AddSetsToSessionExerciseAsync(int sessionExerciseId, ...)` → `(long sessionExerciseId, ...)`

#### 5. Service Implementations
All service implementations in `WorkoutManager.BusinessLogic/Services/Implementations/` that correspond to the interfaces above.

#### 6. Repository Implementations
All repository implementations in `WorkoutManager.BusinessLogic/Services/Implementations/` that correspond to the repository interfaces above.

#### 7. Frontend (Blazor)
- **WorkoutManager.Web/Pages/** - Any Blazor components that parse IDs or pass them to API calls
- Review `PlanDetailPage.razor.cs` and other page code-behind files

## Refactoring Steps (Recommended Order)

### Phase 1: DTOs (Foundation)
**Priority: HIGH** - Changes here propagate to all layers

1. `MuscleGroupDto.cs` - Change `Id` from `int` to `long`
2. `ExerciseDtos.cs` - Change all ID properties from `int` to `long`
3. `SessionDtos.cs` - Change all ID properties from `int` to `long`

**Files to modify:** 3 files, ~10 properties

### Phase 2: Repository Interfaces & Implementations
**Priority: HIGH** - Data layer must be correct before business logic

4. `IExerciseRepository.cs` + `ExerciseRepository.cs`
5. `IMuscleGroupRepository.cs` + `MuscleGroupRepository.cs`
6. `IPlanExerciseRepository.cs` + `PlanExerciseRepository.cs`
7. `ISessionRepository.cs` + `SessionRepository.cs`
8. `ISessionExerciseRepository.cs` + `SessionExerciseRepository.cs`

**Files to modify:** 10 files, ~21 method signatures

### Phase 3: Service Interfaces & Implementations
**Priority: HIGH** - Business logic must match repository layer

9. `IExerciseService.cs` + `ExerciseService.cs`
10. `IMuscleGroupService.cs` + `MuscleGroupService.cs`
11. `IPlanExerciseService.cs` + `PlanExerciseService.cs` ⚠️ **Contains known bug**
12. `ISessionService.cs` + `SessionService.cs`
13. `ISessionExerciseService.cs` + `SessionExerciseService.cs`

**Files to modify:** 10 files, ~18 method signatures

### Phase 4: Controllers (API Layer)
**Priority: MEDIUM** - External interface should match internal types

14. `ExercisesController.cs`
15. `MuscleGroupsController.cs`
16. `SessionsController.cs`
17. `SessionExercisesController.cs`
18. `PlanDayExercisesController.cs`

**Files to modify:** 5 files, ~13 method signatures

### Phase 5: Frontend (Blazor)
**Priority: MEDIUM** - UI must parse and pass correct types

19. Review all `.razor.cs` files in `WorkoutManager.Web/Pages/`
20. Search for `int.Parse()` or similar ID parsing
21. Update API call parameter types
22. Review `PlanDetailPage.razor.cs` (currently open in IDE)

**Files to modify:** TBD (requires detailed frontend analysis)

### Phase 6: Testing & Validation
**Priority: HIGH** - Ensure no regressions

23. Run all unit tests in:
    - `WorkoutManager.Api.Tests/`
    - `WorkoutManager.BusinessLogic.Tests/`
    - `WorkoutManager.Data.Tests/`
24. Run E2E tests in `WorkoutManager.E2E.Tests/`
25. Manual testing of critical user flows
26. Database integration testing with large ID values

## Known Issues to Fix

### Critical Bug in PlanExerciseService.cs:30
```csharp
// Current (BROKEN):
public async Task AddExerciseToDayAsync(int planId, int dayId, ...)
{
    if (await _workoutPlanService.IsPlanLockedAsync(planId, userId))
    // ❌ Passing int to method expecting long
}
```

This will be automatically fixed when implementing Phase 3.

## Impact Analysis

### Breaking Changes
- ✅ **Internal APIs**: Changes are internal to the application
- ✅ **External APIs**: Route parameters change from `{id:int}` to `{id:long}` (backwards compatible - `long` accepts `int` values)
- ✅ **Database**: No schema changes required (already using `bigint`/`long`)
- ⚠️ **Frontend**: May require updates to ID parsing and type declarations

### Risk Assessment
- **Risk Level**: MEDIUM
- **Compilation Impact**: HIGH (many files affected, but compiler will catch errors)
- **Runtime Impact**: LOW (if compiled successfully, behavior should be identical for existing IDs)
- **Data Loss Risk**: NONE (no database changes)

## Testing Strategy

### Unit Tests
1. Verify all service methods accept `long` IDs
2. Verify all repository methods accept `long` IDs
3. Test with edge cases:
   - `long.MaxValue` (9,223,372,036,854,775,807)
   - `int.MaxValue + 1` (2,147,483,648)
   - Negative IDs (if applicable)

### Integration Tests
1. API endpoint tests with `long` ID values
2. Database round-trip tests with large IDs
3. DTO serialization/deserialization tests

### E2E Tests
1. Full user workflows with various ID values
2. Create, read, update, delete operations
3. Complex scenarios (nested resources, relationships)

## Rollback Plan

If issues arise during refactoring:

1. **Git Branch Strategy**: Perform refactoring in feature branch `refactor/id-long-type`
2. **Atomic Commits**: Commit each phase separately for granular rollback
3. **Feature Flag**: Consider adding feature flag if frontend changes are risky
4. **Database**: No rollback needed (schema unchanged)

## Success Criteria

- [ ] All compilation warnings/errors resolved
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] All E2E tests passing
- [ ] Code review completed
- [ ] No runtime errors in staging environment
- [ ] Performance benchmarks unchanged

## Estimated Effort

- **DTOs**: 1 hour
- **Repositories**: 3 hours
- **Services**: 3 hours
- **Controllers**: 2 hours
- **Frontend**: 2-4 hours
- **Testing**: 4 hours
- **Code Review & Fixes**: 2 hours

**Total: 17-19 hours** (2-3 days)

## Implementation Notes

### Search & Replace Strategy
Do NOT use global find/replace. Use surgical, file-by-file changes to avoid false positives (e.g., pagination `int Page`, `int PageSize`).

### Compiler as Guide
Let the compiler identify all affected call sites after changing interfaces. This ensures no references are missed.

### Test-Driven Approach
Update tests first for each layer, then implement changes to make tests pass.

## Related Files

- **Tech Stack**: `.ai/tech-stack.md`
- **Repository Pattern**: `.ai/refactor-repository-pattern.md`
- **Test Plan**: `.ai/test-plan.md`

## Questions to Resolve

1. Are there any external API consumers that might be affected by route changes?
2. Should we add validation to ensure IDs are positive values?
3. Are there any database views or stored procedures that need updating?
4. Should we add automated tests to prevent future type regressions?
