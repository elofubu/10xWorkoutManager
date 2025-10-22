# Solution: Linking Sessions to Training Days

## Problem Analysis

The original schema had a critical design flaw: the `sessions` table only linked to `workout_plans` via `plan_id`, but not to specific `training_days`. This meant:

- ❌ No way to track which specific training day (e.g., "Day A - Push", "Day B - Pull") was performed
- ❌ Cannot show proper session history grouped by training day
- ❌ Cannot recommend which day to do next in rotation
- ❌ Cannot track progression for specific training days
- ❌ Cannot display "Last performed: X days ago" for each training day

## Solution Implemented

### Design Decision: Required Training Day Link

**Every session MUST be linked to a training day.** There are no ad-hoc sessions - all workouts follow the structured plan.

### 1. Database Schema Changes

**New Migration**: `20251021_add_training_day_to_sessions.sql`

Added `training_day_id` column to the `sessions` table:
- **Type**: `BIGINT NOT NULL` (required)
- **Foreign Key**: References `training_days(id)` with `ON DELETE RESTRICT`
- **Index**: Created `idx_sessions_training_day_id` for query performance

**Complete Migration**:
```sql
-- Add training_day_id column to sessions table
ALTER TABLE public.sessions 
ADD COLUMN training_day_id BIGINT NOT NULL
REFERENCES public.training_days(id) ON DELETE RESTRICT;

-- Create an index for better query performance
CREATE INDEX idx_sessions_training_day_id ON public.sessions(training_day_id);

-- Add comment for documentation
COMMENT ON COLUMN public.sessions.training_day_id IS 'The specific training day performed in this session. Required - every session must be linked to a training day.';
```

### 2. Data Model Updates

#### `Session.cs`
```csharp
[Column("training_day_id")]
public long TrainingDayId { get; set; }  // Required

[JsonIgnore]
public TrainingDay? TrainingDay { get; set; }
```

#### `TrainingDay.cs`
```csharp
[JsonIgnore]
public List<Session> Sessions { get; set; } = new();
```

### 3. DTO Updates

#### `SessionSummaryDto`
```csharp
public class SessionSummaryDto
{
    public long Id { get; set; }
    public long? PlanId { get; set; }
    public long TrainingDayId { get; set; }  // Required
    public string? PlanName { get; set; }
    public string TrainingDayName { get; set; }  // Always populated
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
```

#### `SessionDetailsDto`
```csharp
public class SessionDetailsDto
{
    public long Id { get; set; }
    public long? PlanId { get; set; }
    public long TrainingDayId { get; set; }  // Required
    public string? PlanName { get; set; }
    public string TrainingDayName { get; set; }  // Always populated
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<SessionExerciseDetailsDto> Exercises { get; set; } = new();
}
```

### 4. Service Layer Updates

#### `ISessionService`
```csharp
/// <summary>
/// Starts a new workout session for a specific training day.
/// The session will be linked to the training day and pre-populated with its exercises.
/// </summary>
Task<SessionDetailsDto> StartSessionAsync(long trainingDayId, Guid userId);
```

#### `SessionService.StartSessionAsync`
Simplified implementation (no nullable handling):
```csharp
public async Task<SessionDetailsDto> StartSessionAsync(long trainingDayId, Guid userId)
{
    if (await _sessionRepository.HasActiveSessionAsync(userId))
    {
        throw new BusinessRuleViolationException("You already have an active session.");
    }

    var trainingDay = await _sessionRepository.GetTrainingDayByIdAsync(trainingDayId);
    if (trainingDay == null)
    {
        throw new NotFoundException("TrainingDay", trainingDayId);
    }

    var plan = await _sessionRepository.GetWorkoutPlanByIdAsync(trainingDay.PlanId, userId);
    if (plan == null)
    {
        throw new NotFoundException("WorkoutPlan", trainingDay.PlanId);
    }

    var session = new Session
    {
        UserId = userId,
        PlanId = trainingDay.PlanId,
        TrainingDayId = trainingDayId,
        StartTime = DateTime.UtcNow
    };
    var createdSession = await _sessionRepository.CreateSessionAsync(session);

    // Pre-populate exercises from the training day
    var planDayExercises = await _sessionRepository.GetPlanDayExercisesAsync(trainingDayId);
    // ... create session exercises
    
    return sessionDto;
}
```

#### `SessionRepository`
Enhanced to fetch training day data:
```csharp
public async Task<IEnumerable<Session>> GetSessionHistoryAsync(Guid userId, int page, int pageSize)
{
    // Fetch sessions
    var response = await _supabaseClient
        .From<Session>()
        .Where(s => s.UserId == userId)
        .Order("start_time", Ordering.Descending)
        .Range(from, to)
        .Get();

    // Fetch related training days
    var trainingDayIds = response.Models.Select(s => s.TrainingDayId).Distinct().ToList();
    var trainingDaysResponse = await _supabaseClient
        .From<TrainingDay>()
        .Filter("id", Operator.In, trainingDayIds)
        .Get();

    foreach (var model in response.Models)
    {
        model.TrainingDay = trainingDaysResponse.Models.FirstOrDefault(td => td.Id == model.TrainingDayId);
    }

    return response.Models;
}
```

### 5. Command Updates

#### `StartSessionCommand`
```csharp
public class StartSessionCommand
{
    public int TrainingDayId { get; set; }  // Required
}
```

## Benefits of This Change

1. ✅ **Session History**: Show "Last session for Day A - Push: 5 days ago"
2. ✅ **Smart Recommendations**: "Based on your plan rotation, do Day B next"
3. ✅ **Pre-population**: Auto-populate exercises when starting a session from a training day
4. ✅ **Better Analytics**: Track volume, intensity, and progression per training day
5. ✅ **Data Integrity**: Every session is properly structured and tracked
6. ✅ **Simple Implementation**: No nullable handling, clear business rules

## Updated Relationship Diagram

```
workout_plans (1) ─┬─→ (many) training_days
                   │
                   └─→ (many) sessions
                              ↑
training_days (1) ────────────┘ (via training_day_id - REQUIRED)
```

## Implementation Details

### Why NOT NULL (Required)?
The `training_day_id` is required to ensure:
1. **Complete tracking**: Every session is linked to a structured training day
2. **Better analytics**: All sessions can be analyzed by training day
3. **Simpler implementation**: No nullable handling needed
4. **Clear intent**: Users always know which day they're performing

### ON DELETE RESTRICT
When a user tries to delete a training day that has sessions:
- The delete operation is blocked (RESTRICT)
- User must first delete the sessions if they want to remove the training day
- Protects historical data from accidental deletion
- Provides clear feedback: "Cannot delete training day - sessions exist"

### Performance Considerations
- Created index on `training_day_id` for fast lookups
- Repository efficiently fetches related data to avoid N+1 queries
- Batches plan and training day fetches in `GetSessionHistoryAsync`

## Migration Instructions

To apply this change to an existing database:

1. Run the migration: `20251021_add_training_day_to_sessions.sql`
2. **Important**: If you have existing sessions without `training_day_id`, you need to either:
   - Delete them, OR
   - Manually assign a `training_day_id` to each before running the migration
3. New sessions will automatically require `training_day_id`

## Next Steps

Future enhancements enabled by this change:
- **Smart rotation**: "You last did Day A, next is Day B"
- **Per-day analytics**: "Your bench press has increased 10% on Push day"
- **Training day frequency**: "You do Leg day every 4.2 days on average"
- **Performance trends**: Graph progression for each training day separately
- **Recommendations**: "You haven't done Pull day in 7 days"

## Key Files Changed

### Code
- `WorkoutManager.BusinessLogic/Commands/SessionCommands.cs` - Required TrainingDayId
- `WorkoutManager.Data/Models/Session.cs` - Added TrainingDayId (NOT NULL)
- `WorkoutManager.Data/Models/TrainingDay.cs` - Added Sessions navigation
- `WorkoutManager.BusinessLogic/DTOs/SessionDtos.cs` - Added training day fields
- `WorkoutManager.BusinessLogic/Services/Interfaces/ISessionService.cs` - Required parameter
- `WorkoutManager.BusinessLogic/Services/Implementations/SessionService.cs` - Simplified logic
- `WorkoutManager.BusinessLogic/Services/Implementations/SessionRepository.cs` - Fetch training day data

### Database
- `WorkoutManager.Data/supabase/migrations/20251021_add_training_day_to_sessions.sql` - NOT NULL migration

### Documentation
- `.ai/db-plan.md` - Updated relationships
- `.ai/business-logic-plan.md` - Updated service signatures
