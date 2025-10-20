# TrainingDay Many-to-Many Refactor Plan

## 1. Context & Problem Statement

**Current Situation:**
- `plan_day_exercises` table now has composite primary key: `(id, training_day_id, exercise_id)`
- `TrainingDay` model updated with direct `Exercises` navigation property
- This creates a many-to-many relationship via the junction table
- **Problem**: Direct `Exercises` navigation loses the `order` property stored in `plan_day_exercises`

**Goal:**
- Refactor the business layer to properly handle M2M relationships
- Preserve ordering information for exercises within training days
- Maintain backward compatibility with existing features
- Simplify repository queries using Supabase C# M2M capabilities

---

## 2. Database Schema Changes

### 2.1 plan_day_exercises Table

**Current Primary Key:** `id` (single)
**New Primary Key:** `(id, training_day_id, exercise_id)` (composite)

```sql
ALTER TABLE public.plan_day_exercises
  DROP CONSTRAINT plan_day_exercises_pkey,
  ADD PRIMARY KEY (id, training_day_id, exercise_id);
```

**Rationale:**
- Enables true M2M semantics in Supabase C# library
- Composite key ensures uniqueness of the relationship
- Maintains individual `id` for potential direct access

---

## 3. Model Layer Refactoring

### 3.1 TrainingDay.cs - Option A (Keep Both Relations)

**Current:**
```csharp
[Reference(typeof(Exercise), useInnerJoin: false, includeInQuery: true)]
public List<Exercise> Exercises { get; set; }

// Commented out - needs to be re-enabled
// [Reference(typeof(PlanDayExercise))]
// public List<PlanDayExercise> PlanDayExercises { get; set; }
```

**Proposed:**
```csharp
// Direct M2M to Exercise for simple queries
[Reference(typeof(Exercise), useInnerJoin: false, includeInQuery: true)]
public List<Exercise> Exercises { get; set; } = new();

// Junction table for order/metadata access
[JsonIgnore]
[Reference(typeof(PlanDayExercise))]
public List<PlanDayExercise> PlanDayExercises { get; set; } = new();
```

**Rationale:**
- Keep both properties for maximum flexibility
- `Exercises` - used for simple UI lists where order is handled separately
- `PlanDayExercises` - used when order information is needed (drag-drop, reordering)
- More compatible with Supabase C# library M2M handling
- Easier to debug and understand data flow

### 3.2 PlanDayExercise.cs

**Update to reflect new composite key:**
```csharp
[PrimaryKey("id", false)]
public long Id { get; set; }

[Column("training_day_id")]
public long TrainingDayId { get; set; }

[Column("exercise_id")]
public long ExerciseId { get; set; }

// Navigation properties (important for Supabase M2M)
[Reference(typeof(Exercise))]
public Exercise? Exercise { get; set; }

[Reference(typeof(TrainingDay))]
public TrainingDay? TrainingDay { get; set; }

[Column("order")]
public short Order { get; set; }

[Column("created_at")]
public DateTime CreatedAt { get; set; }
```

---

## 4. Repository Layer Refactoring

### 4.1 WorkoutPlanRepository.cs

**Update GetTrainingDaysWithExercisesAsync():**

```csharp
public async Task<IEnumerable<TrainingDay>> GetTrainingDaysWithExercisesAsync(long planId)
{
    // Fetch training days with both Exercise relations and PlanDayExercise (for order)
    var trainingDaysResponse = await _supabaseClient
        .From<TrainingDay>()
        .Select("*, exercises(*), plan_day_exercises(*, exercises(*))")
        .Filter("plan_id", Supabase.Postgrest.Constants.Operator.Equals, planId)
        .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
        .Get();

    var trainingDays = trainingDaysResponse.Models
        .OrderBy(td => td.Order)
        .ToList();

    // Sort exercises by order within each training day (from PlanDayExercises)
    foreach (var day in trainingDays)
    {
        day.PlanDayExercises = day.PlanDayExercises
            .OrderBy(pde => pde.Order)
            .ToList();
    }

    return trainingDays;
}
```

### 4.2 New PlanExerciseRepository.cs

Handle all `plan_day_exercises` CRUD operations:

```csharp
public class PlanExerciseRepository : IPlanExerciseRepository
{
    private readonly Client _supabaseClient;

    public PlanExerciseRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<PlanDayExercise> AddExerciseToTrainingDayAsync(
        long trainingDayId, long exerciseId, short order)
    {
        var planDayExercise = new PlanDayExercise
        {
            TrainingDayId = trainingDayId,
            ExerciseId = exerciseId,
            Order = order
        };

        var response = await _supabaseClient
            .From<PlanDayExercise>()
            .Insert(planDayExercise);

        return response.Models.First();
    }

    public async Task RemoveExerciseFromTrainingDayAsync(
        long trainingDayId, long exerciseId)
    {
        await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.TrainingDayId == trainingDayId && pde.ExerciseId == exerciseId)
            .Delete();
    }

    public async Task ReorderExercisesAsync(
        long trainingDayId, IEnumerable<(long ExerciseId, short Order)> exercises)
    {
        foreach (var (exerciseId, order) in exercises)
        {
            var pde = await _supabaseClient
                .From<PlanDayExercise>()
                .Where(p => p.TrainingDayId == trainingDayId && p.ExerciseId == exerciseId)
                .Single();

            if (pde != null)
            {
                pde.Order = order;
                await _supabaseClient.From<PlanDayExercise>().Update(pde);
            }
        }
    }

    public async Task<IEnumerable<(Exercise Exercise, short Order)>> GetExercisesWithOrderAsync(
        long trainingDayId)
    {
        var planDayExercisesResponse = await _supabaseClient
            .From<PlanDayExercise>()
            .Select("*, exercises(*)")
            .Filter("training_day_id", Supabase.Postgrest.Constants.Operator.Equals, trainingDayId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        return planDayExercisesResponse.Models
            .Where(pde => pde.Exercise != null)
            .Select(pde => (pde.Exercise, pde.Order));
    }
}
```

---

## 5. Business Logic Layer Refactoring

### 5.1 Updated IPlanExerciseService

```csharp
public interface IPlanExerciseService
{
    // Get exercises for a training day WITH ordering info
    Task<IEnumerable<PlanExerciseDetailDto>> GetExercisesForTrainingDayAsync(long trainingDayId);

    // Add exercise to training day
    Task<PlanExerciseDetailDto> AddExerciseToTrainingDayAsync(
        long trainingDayId, long exerciseId);

    // Remove exercise from training day
    Task RemoveExerciseFromTrainingDayAsync(long trainingDayId, long exerciseId);

    // Reorder exercises within training day
    Task ReorderExercisesAsync(long trainingDayId, IEnumerable<long> orderedExerciseIds);
}
```

### 5.2 New PlanExerciseDetailDto

```csharp
public class PlanExerciseDetailDto
{
    public long ExerciseId { get; set; }
    public string ExerciseName { get; set; }
    public long MuscleGroupId { get; set; }
    public string MuscleGroupName { get; set; }
    public short Order { get; set; }  // Order within the training day
}
```

### 5.3 PlanExerciseService Implementation

```csharp
public class PlanExerciseService : IPlanExerciseService
{
    private readonly IPlanExerciseRepository _planExerciseRepository;
    private readonly IMapper _mapper;

    public PlanExerciseService(IPlanExerciseRepository planExerciseRepository, IMapper mapper)
    {
        _planExerciseRepository = planExerciseRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PlanExerciseDetailDto>> GetExercisesForTrainingDayAsync(
        long trainingDayId)
    {
        var exercisesWithOrder = await _planExerciseRepository
            .GetExercisesWithOrderAsync(trainingDayId);

        return exercisesWithOrder
            .Select(x => new PlanExerciseDetailDto
            {
                ExerciseId = x.Exercise.Id,
                ExerciseName = x.Exercise.Name,
                MuscleGroupId = x.Exercise.MuscleGroupId,
                Order = x.Order
            })
            .ToList();
    }

    public async Task<PlanExerciseDetailDto> AddExerciseToTrainingDayAsync(
        long trainingDayId, long exerciseId)
    {
        // Get max order for this training day
        var existingExercises = await _planExerciseRepository
            .GetExercisesWithOrderAsync(trainingDayId);

        var nextOrder = (short)(existingExercises.Any()
            ? existingExercises.Max(x => x.Order) + 1
            : 0);

        var planDayExercise = await _planExerciseRepository
            .AddExerciseToTrainingDayAsync(trainingDayId, exerciseId, nextOrder);

        return new PlanExerciseDetailDto
        {
            ExerciseId = planDayExercise.Exercise.Id,
            ExerciseName = planDayExercise.Exercise.Name,
            MuscleGroupId = planDayExercise.Exercise.MuscleGroupId,
            Order = planDayExercise.Order
        };
    }

    public async Task RemoveExerciseFromTrainingDayAsync(long trainingDayId, long exerciseId)
    {
        await _planExerciseRepository
            .RemoveExerciseFromTrainingDayAsync(trainingDayId, exerciseId);
    }

    public async Task ReorderExercisesAsync(long trainingDayId, IEnumerable<long> orderedExerciseIds)
    {
        var orderedList = orderedExerciseIds.ToList();
        var exercisesToReorder = orderedList
            .Select((id, index) => (id, (short)index))
            .ToList();

        await _planExerciseRepository.ReorderExercisesAsync(trainingDayId, exercisesToReorder);
    }
}
```

---

## 6. Test Layer Updates

### 6.1 Updated Repository Tests

Test cases for new M2M query methods:
- `GetTrainingDaysWithExercisesAsync_ReturnsExercisesOrderedCorrectly()`
- `GetExercisesWithOrderAsync_IncludesOrderProperty()`
- `AddExerciseToTrainingDayAsync_AssignsCorrectOrder()`
- `ReorderExercisesAsync_UpdatesAllOrders()`

### 6.2 Integration Tests

- Exercise addition maintains order
- Exercise removal updates remaining orders
- Reordering correctly updates all order values
- Querying preserves order after updates
- Both `Exercises` and `PlanDayExercises` populated correctly

---

## 7. Migration Steps (Implementation Order)

### Phase 1: Database
1. Create migration: `20251020_update_plan_day_exercises_pk.sql`
2. Update `plan_day_exercises` primary key to composite
3. Verify foreign key constraints still valid

### Phase 2: Models
1. Update `TrainingDay.cs` - re-enable `PlanDayExercises` property
2. Verify `PlanDayExercise.cs` has all required attributes
3. Verify `Exercise.cs` has all required attributes

### Phase 3: Data Access
1. Create `IPlanExerciseRepository` interface
2. Create `PlanExerciseRepository` implementation
3. Update `WorkoutPlanRepository.GetTrainingDaysWithExercisesAsync()`

### Phase 4: Business Logic
1. Create `PlanExerciseDetailDto`
2. Update/Create `IPlanExerciseService` interface
3. Implement `PlanExerciseService`
4. Register dependencies in DI container

### Phase 5: Presentation (Blazor)
1. Update components to use new `PlanDayExercises` for ordering
2. Test ordering UI rendering
3. Test drag-and-drop reordering

### Phase 6: Testing
1. Update unit tests
2. Add integration tests
3. Add E2E tests

---

## 8. Benefits of This Approach

✅ **True M2M Relations**: Leverages Supabase C# M2M capabilities with composite keys
✅ **Order Preservation**: Maintains order information via `PlanDayExercises` property
✅ **Single Query**: Can fetch all needed data in one Supabase call
✅ **Dual Access**: Use `Exercises` for simple lists, `PlanDayExercises` for ordering
✅ **Type Safety**: Strong DTOs prevent errors
✅ **Backward Compatible**: Existing features continue to work
✅ **Testable**: Clear separation of concerns
✅ **Maintainable**: Standard M2M pattern familiar to .NET developers

---

## 9. Potential Issues & Mitigation

| Issue | Mitigation |
|-------|-----------|
| Performance with large datasets | Add pagination, use projections to limit columns |
| Order gaps after deletions | Implement reorder operation to compact order values |
| Cascading deletes of exercises | Use `ON DELETE RESTRICT` on exercise FK |
| Null exercise references in PlanDayExercises | Filter out nulls, add validation |
| JSON serialization circular refs | Use `[JsonIgnore]` on back-reference properties |
| Stale data in Exercises vs PlanDayExercises | Load both from same query to ensure consistency |

---

## 10. Related Files to Update

**Models:**
- `WorkoutManager.Data/Models/TrainingDay.cs`
- `WorkoutManager.Data/Models/PlanDayExercise.cs`

**Repository:**
- `WorkoutManager.BusinessLogic/Services/Implementations/WorkoutPlanRepository.cs`
- `WorkoutManager.BusinessLogic/Services/Implementations/PlanExerciseRepository.cs` (new)
- `WorkoutManager.BusinessLogic/Services/Interfaces/IPlanExerciseRepository.cs` (new)

**Business Logic:**
- `WorkoutManager.BusinessLogic/Services/Interfaces/IPlanExerciseService.cs`
- `WorkoutManager.BusinessLogic/Services/Implementations/PlanExerciseService.cs` (new)
- DTOs: `PlanExerciseDetailDto.cs` (new)

**Database:**
- `WorkoutManager.Data/Supabase/Migrations/20251020_update_plan_day_exercises_pk.sql` (new)

**Tests:**
- `WorkoutManager.BusinessLogic.Tests/Services/PlanExerciseRepositoryTests.cs` (new)
- `WorkoutManager.BusinessLogic.Tests/Services/PlanExerciseServiceTests.cs` (new)
- Updated: existing repository and service tests

---

## 11. Success Criteria

- ✅ All training days load with both `Exercises` and `PlanDayExercises` populated
- ✅ Exercises are loaded in correct order from `PlanDayExercises`
- ✅ Adding exercises maintains proper ordering
- ✅ Reordering exercises persists correctly
- ✅ Single Supabase query fetches all needed data
- ✅ No `PlanDayExercises` collection is null
- ✅ All existing tests pass
- ✅ New M2M tests cover edge cases
- ✅ E2E tests verify UI ordering works correctly
- ✅ Order information accessible in UI for drag-and-drop
