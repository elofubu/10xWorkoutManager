# WorkoutManager.BusinessLogic Refactoring Plan

## Executive Summary

Analysis of the WorkoutManager.BusinessLogic folder identified 5 files with the highest complexity (LOC). Key findings reveal systemic issues with:
- **N+1 Query Problems** in data access layer (3 repositories affected)
- **Code Duplication** in business logic (authorization/validation patterns)
- **Manual Data Mapping** causing tight coupling
- **Inefficient In-Memory Operations** when database-level filtering is available

**Overall Impact**: Medium-High. These issues affect performance, maintainability, and testability.

---

## Detailed Issues & Fix Plans

### FILE 1: SessionService.cs (181 LOC)
**Path**: `WorkoutManager.BusinessLogic/Services/Implementations/SessionService.cs`
**Severity**: MEDIUM (Code Quality & Maintainability)

#### Issues Detected:

1. **Repeated Null-Check Pattern (5 occurrences)**
   - Lines: 30-33, 36-39, 102-105, 138-141, 150-153
   - Description: Same null-checking logic duplicated across methods
   - Impact: Violates DRY principle, increases maintenance burden

   **Fix Steps:**
   ```
   1. Create private helper method: ThrowIfNotFound<T>(Func<Task<T?>> fetch, string entityName, object id)
   2. Replace all manual null checks with calls to this method
   3. Add unit tests for the helper method using xUnit + FluentAssertions
   ```

2. **Scattered DTO Mapping Logic**
   - Lines: 62-68 (SessionExerciseDetailsDto), 108-123 (with nested mapping), 125-132
   - Description: DTO creation mixed with business logic
   - Impact: Hard to modify DTOs, difficult to test mapping independently

   **Fix Steps:**
   ```
   1. Create ISessionMapper interface in Services/Interfaces/
   2. Create SessionMapper implementation in Services/Implementations/
   3. Add methods: MapToSessionExerciseDetailsDto(), MapToSessionDetailsDto()
   4. Inject ISessionMapper into SessionService
   5. Update SessionService methods to use mapper
   6. Add unit tests for mapper using xUnit
   ```

3. **Sequential Repository Calls (Potential Performance Issue)**
   - Line 49-69: Creates session, then loops creating session exercises (N+1 pattern)
   - Impact: Multiple database round-trips
   - Conditions: Critical if many exercises per training day

   **Fix Steps:**
   ```
   1. Evaluate if batch insert is supported by Supabase SDK
   2. If supported: Collect all SessionExercise objects, insert in single call
   3. If not: Consider caching frequently accessed training day exercises
   4. Add performance test using k6 (from tech stack) to measure impact
   ```

---

### FILE 2: SessionRepository.cs (156 LOC)
**Path**: `WorkoutManager.BusinessLogic/Services/Implementations/SessionRepository.cs`
**Severity**: HIGH (Performance)

#### Issues Detected:

1. **N+1 Query Pattern in GetSessionHistoryAsync (Lines 65-98)** ✅ COMPLETED
   - Query 1: Fetch sessions by user
   - Query 2: Fetch plans for those sessions (separate call)
   - Description: Avoidable second database round-trip
   - Impact: Performance degradation with many sessions
   - **COMPLETED**: Updated to use `Select("*, workout_plans(*)")` for nested query projection

   **Fix Steps:**
   ```
   1. Update query to use Supabase relation/select feature: Select("*, workout_plans(*)")  ✅ DONE
   2. Test with WebApplicationFactory (from tech stack)
   3. Benchmark before/after using Alba (HTTP endpoint testing)
   4. Add integration test ensuring Plan data is populated
   ```

2. **N+1 Query Pattern in GetSessionExercisesWithSetsAsync (Lines 109-137)**
   - Query 1: Fetch session exercises
   - Query 2: Fetch exercise sets for all exercises (separate call)
   - Description: Complex loop with manual relationship loading
   - Impact: Scales poorly as session complexity increases

   **Fix Steps:**
   ```
   1. Replace with single query using projections: Select("*, exercise_sets(*)")
   2. Simplify loops by removing manual grouping logic
   3. Add integration tests using WebApplicationFactory
   4. Add Verify.Http snapshot tests for API contracts
   ```

3. **Repeated Supabase Filter Syntax**
   - Lines 21, 92-94: Inconsistent filter patterns
   - Description: Filter syntax varies across methods
   - Impact: Code readability, harder to maintain consistency

   **Fix Steps:**
   ```
   1. Create extension methods in a new file: SupabaseFilterExtensions.cs
   2. Add method: IsNullFilter<T>(this IQueryable<T>, selector)
   3. Replace all Filter<DateTime?>(s => s.EndTime, Equals, null) with IsNullFilter()
   4. Add unit tests for extension methods
   ```

---

### FILE 3: WorkoutPlanRepository.cs (153 LOC)
**Path**: `WorkoutManager.BusinessLogic/Services/Implementations/WorkoutPlanRepository.cs`
**Severity**: HIGH (Performance & Complexity)

#### Issues Detected:

1. **Severe N+1 Query Problem in GetTrainingDaysWithExercisesAsync (Lines 99-151)**
   - Query 1: Fetch training days by plan (line 101-105)
   - Query 2: Fetch plan day exercises (line 115-118)
   - Query 3: Fetch exercises by IDs (line 128-131)
   - Description: 3 sequential queries building hierarchy manually
   - Impact: Significant performance degradation, especially with large plans

   **Fix Steps:**
   ```
   1. Create new method using Supabase nested projections:
      Select("*, training_days(*, plan_day_exercises(*, exercises(*)))")
   2. Replace entire GetTrainingDaysWithExercisesAsync method body
   3. Remove manual grouping/lookup logic (lines 135-148)
   4. Add integration tests with multiple training days and exercises
   5. Performance test: Compare old vs new with k6 (10+ training days scenario)
   ```

2. **Complex Manual Relationship Loading (Lines 135-148)**
   - Description: Nested loops with dictionary lookups
   - Impact: Fragile, error-prone, hard to debug

   **Fix Steps:**
   ```
   1. This will be eliminated when fixing Issue #1
   2. After refactoring, code will be declarative not imperative
   ```

3. **Inconsistent Filter Syntax (Line 93 vs others)**
   - Line 93: Uses string literal "plan_id"
   - Lines elsewhere: Use strongly-typed property access
   - Description: Mixed filter approaches
   - Impact: Code inconsistency, potential bugs with typos

   **Fix Steps:**
   ```
   1. Apply the SupabaseFilterExtensions from SessionRepository fix
   2. Use consistent strongly-typed filters throughout
   3. Add code review checklist item for filter consistency
   ```

---

### FILE 4: PlanExerciseService.cs (113 LOC)
**Path**: `WorkoutManager.BusinessLogic/Services/Implementations/PlanExerciseService.cs`
**Severity**: MEDIUM (Maintainability & Testing)

#### Issues Detected:

1. **Repeated Authorization Pattern (3 occurrences)**
   - Lines: 29-30, 66-67, 89-90
   - Description: Same ownership verification called in every public method
   - Impact: Violates DRY, authorization logic scattered

   **Fix Steps:**
   ```
   1. Consolidate into single method: EnsurePlanAccessibleAsync(long planId, Guid userId)
   2. Combine VerifyPlanOwnershipAsync + IsPlanLockedAsync checks
   3. Create decorator attribute if using ASP.NET Core: [AuthorizePlanOwner]
   4. Test with FluentAssertions: verify exception type and message
   ```

2. **Repeated Lock-Checking Business Rule (3 occurrences)**
   - Lines: 30-32, 67-69, 90-92
   - Description: Identical "Cannot modify locked plan" logic
   - Impact: Business rule defined in multiple places (maintenance risk)

   **Fix Steps:**
   ```
   1. Create IModificationPolicy interface in Services/Interfaces/
   2. Implement LockedPlanModificationPolicy in Services/Implementations/
   3. Inject into service constructor
   4. Call policy.CanModifyPlanAsync() once instead of 3 times
   5. Add unit tests for policy using xUnit + Bogus (test data generation)
   ```

3. **Repeated Entity Existence Checks**
   - Lines: 35-45 (trainingDay + exercise), 72-82 (trainingDay + planDayExercise)
   - Description: Similar null-check patterns
   - Impact: Maintenance burden if validation logic changes

   **Fix Steps:**
   ```
   1. Use helper method from SessionService fix: ThrowIfNotFound<T>()
   2. Replace all manual null checks with single helper call
   3. This promotes code reuse across services
   ```

---

### FILE 5: ExerciseRepository.cs (99 LOC)
**Path**: `WorkoutManager.BusinessLogic/Services/Implementations/ExerciseRepository.cs`
**Severity**: HIGH (Performance)

#### Issues Detected:

1. **N+1 Query Pattern in GetLastPerformanceAsync (Lines 58-98)**
   - Query 1: Fetch session exercises (line 60-63)
   - Query 2: Fetch matching sessions (line 69-74)
   - Query 3: Fetch exercise sets (line 81-85)
   - Description: 3 separate queries to build single DTO
   - Impact: Significant performance issue on frequently called method

   **Fix Steps:**
   ```
   1. Optimize using single query with projections:
      Select("*, session(*, exercise_sets(*))")
   2. Use database ordering instead of LINQ: Order("session.start_time", DESC)
   3. Eliminate manual sorting: Remove OrderByDescending().First()
   4. Add integration tests with multiple sessions and sets
   5. Add performance regression test using Alba
   ```

2. **In-Memory Filtering After Full Table Scan (Line 27)**
   - Description: Fetches ALL exercises, then filters in-memory
   - Impact: Wasteful with large exercise databases

   **Fix Steps:**
   ```
   1. Use Supabase OR filter at database level:
      .Or($"user_id.eq.{userId},user_id.is.null")
   2. Remove in-memory .Where() clause
   3. Test with large dataset (500+ exercises)
   4. Verify index on user_id column exists in Supabase
   ```

3. **Inefficient Result Selection**
   - Line 78-79: `OrderByDescending(s => s.StartTime).First()` after loading all sessions
   - Description: Loads multiple sessions then sorts in-memory
   - Impact: Poor query planning, should order at database level

   **Fix Steps:**
   ```
   1. Already covered in Issue #1 fix
   2. Use database ORDER BY LIMIT 1 instead
   ```

---

## Implementation Timeline & Priority Matrix

### PRIORITY 1 (CRITICAL - Start Immediately)
**Target**: Performance bottlenecks affecting user experience
- [x] Fix N+1 queries in SessionRepository.GetSessionHistoryAsync (COMPLETED: Using `Select("*, workout_plans(*)")`)
- [ ] Fix N+1 queries in WorkoutPlanRepository.GetTrainingDaysWithExercisesAsync
- [ ] Fix N+1 queries in ExerciseRepository.GetLastPerformanceAsync

**Effort**: 8 hours | **Impact**: HIGH | **Risk**: LOW

**Definition of Done**:
- Queries reduced from N+1 to 1
- Integration tests pass
- Performance benchmarks show improvement
- No data loss or behavioral changes

---

### PRIORITY 2 (HIGH - Complete in Sprint) - PLANNED
**Target**: Code quality and maintainability
- [ ] Create helper method ThrowIfNotFound<T> (shared across services)
- [ ] Create ISessionMapper and implement mapping logic
- [ ] Create SupabaseFilterExtensions for consistent filtering
- [ ] Create IModificationPolicy and apply to PlanExerciseService

**Effort**: 10 hours | **Impact**: MEDIUM-HIGH | **Risk**: LOW

**Status**: READY FOR IMPLEMENTATION - See `High-Implementation-Plan.md` for detailed plan

**Definition of Done**:
- All unit tests pass (xUnit + FluentAssertions)
- Code coverage > 80% for new helpers
- No behavioral changes to existing functionality
- All 10+ test cases passing
- Code duplication reduced by 50%+

---

### PRIORITY 3 (MEDIUM - Complete in Next Sprint)
**Target**: Testing & Documentation
- [ ] Add performance tests using k6 for repositories
- [ ] Add Verify.Http snapshot tests for API contracts
- [ ] Add WebApplicationFactory integration tests
- [ ] Update architecture documentation

**Effort**: 6 hours | **Impact**: MEDIUM | **Risk**: MEDIUM

**Definition of Done**:
- Performance benchmarks documented
- Regression tests added to CI/CD
- Architecture tests pass (NetArchTest from tech stack)

---

## Testing Strategy

### Unit Tests (xUnit)
```csharp
// Test helper methods
[Fact]
public async Task ThrowIfNotFound_WhenNotFound_ThrowsNotFoundException()
{
    var result = await Assert.ThrowsAsync<NotFoundException>(
        () => service.ThrowIfNotFound<Session>(() => Task.FromResult<Session>(null), "Session", 1)
    );
}

// Test mappers
[Fact]
public void SessionMapper_MapToSessionDetailsDto_MapsAllProperties()
{
    var session = new Faker<Session>().Generate();
    var dto = mapper.MapToSessionDetailsDto(session);

    dto.Should().NotBeNull();
    dto.Id.Should().Be((int)session.Id);
}
```

### Integration Tests (WebApplicationFactory + Alba)
```csharp
// Test queries with database
[Fact]
public async Task GetTrainingDaysWithExercises_ReturnsCompleteHierarchy()
{
    var result = await repository.GetTrainingDaysWithExercisesAsync(planId);

    result.Should().NotBeEmpty();
    result.SelectMany(td => td.PlanDayExercises)
        .Should().NotBeEmpty();
}
```

### Performance Tests (k6)
```javascript
// Load test for session history endpoint
export default function() {
  http.get(`/api/sessions/history?page=1&pageSize=20`);
}
```

---

## Risk Assessment & Mitigation

| Risk | Probability | Severity | Mitigation |
|------|------------|----------|-----------|
| Supabase SDK doesn't support needed projections | Medium | High | Research SDK docs early, create POC |
| Breaking existing API contracts | Low | High | Use Verify.Http snapshot tests |
| Performance regression during refactoring | Medium | Medium | Run performance tests before/after |
| Incomplete data mapping in new mappers | Medium | Medium | Test all DTO properties individually |

---

## Success Metrics

1. **Performance**: Query count reduced from 3 to 1 in top 3 repositories
2. **Code Quality**: Cyclomatic complexity reduced by 30%
3. **Maintainability**: Code duplication reduced by 50%
4. **Test Coverage**: Maintain >80% coverage on new/modified code
5. **CI/CD**: All tests pass, no new SonarQube/SecurityCodeScan issues

---

## References & Tech Stack Alignment

- **Testing Framework**: xUnit (standard for .NET)
- **Assertions**: FluentAssertions (readable, extensive)
- **Test Data**: Bogus (realistic test data generation)
- **Integration Testing**: WebApplicationFactory + Alba
- **Performance Testing**: k6 (load testing)
- **Code Quality**: NetArchTest (layered architecture), SonarQube
- **Snapshot Testing**: Verify.Http (API contracts)

---

## Next Steps

1. **Review**: Share plan with team for feedback
2. **Prioritize**: Confirm Priority 1 items alignment with sprint planning
3. **POC**: Create proof-of-concept for Supabase projection queries
4. **Assign**: Distribute tasks among team members
5. **Track**: Use GitHub Issues for tracking implementation progress

---

**Plan Created**: 2025-10-18
**Last Updated**: 2025-10-19
**Status**: IN PROGRESS - Critical Fix #1 Completed (SessionRepository.GetSessionHistoryAsync)
