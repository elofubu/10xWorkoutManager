# Business Logic Gap Analysis

## Current State Assessment

### ✅ What Exists

#### 1. Data Models (Complete)
All database models exist in `WorkoutManager.Data/Models/`:
- ✅ `WorkoutPlan` - properly annotated with Postgrest attributes
- ✅ `TrainingDay` - includes plan_id, order
- ✅ `PlanDayExercise` - junction table for exercises in days
- ✅ `Exercise` - supports global and user-created exercises
- ✅ `MuscleGroup` - exercise categories
- ✅ `Session` - workout session records
- ✅ `SessionExercise` - exercises performed in session
- ✅ `ExerciseSet` - individual sets with weight/reps

**Status**: All models ready for use with Supabase SDK.

#### 2. DTOs (Mostly Complete)
Located in `WorkoutManager.BusinessLogic/DTOs/`:
- ✅ `WorkoutPlanDtos.cs` - All plan-related DTOs
- ✅ `SessionDtos.cs` - All session-related DTOs
- ✅ `ExerciseDtos.cs` - Exercise DTOs
- ✅ `MuscleGroupDto.cs` - Muscle group DTO
- ✅ `UserDto.cs` - User information
- ✅ `PaginationDtos.cs` - Pagination support

**Minor Gaps**:
- Missing `CreateSessionDto` (currently using anonymous objects)
- Missing some specialized query DTOs

#### 3. Commands (Partial)
Located in `WorkoutManager.BusinessLogic/Commands/`:
- ✅ `WorkoutPlanCommands.cs` - Create, Update, Reorder commands
- ✅ `SessionCommands.cs` - Start, Update session commands
- ✅ `CreateExerciseCommand.cs` - Create exercise command
- ⚠️ MediatR package is installed but NO handlers implemented
- ⚠️ Commands exist but aren't being used (controllers use DTOs directly)

### ❌ What's Missing (Critical Gaps)

#### 1. Service Interfaces (Completely Missing)
**Location**: `WorkoutManager.BusinessLogic/Services/Interfaces/` (empty folder)

According to business-logic-plan.md, we need:
- ❌ `IWorkoutPlanService`
- ❌ `ITrainingDayService`
- ❌ `IPlanExerciseService`
- ❌ `IExerciseService`
- ❌ `ISessionService`
- ❌ `ISessionExerciseService`
- ❌ `IExerciseSetService`
- ❌ `IExerciseHistoryService`
- ❌ `IMuscleGroupService`

#### 2. Service Implementations (Completely Missing)
**Location**: `WorkoutManager.BusinessLogic/Services/` (no implementations exist)

All services need to be implemented with:
- Supabase client integration
- CRUD operations
- Business logic enforcement
- Error handling
- Data validation

#### 3. Validators (Not Implemented)
**Location**: `WorkoutManager.BusinessLogic/Validators/` (folder exists but empty)

Need validators for:
- ❌ CreateWorkoutPlanCommandValidator
- ❌ UpdateWorkoutPlanCommandValidator
- ❌ CreateExerciseCommandValidator
- ❌ SessionCommandValidators
- ❌ ExerciseSetValidators

Should use **FluentValidation** as mentioned in business-logic-plan.md.

#### 4. Mappers (Not Implemented)
No mapping configuration exists for:
- Model → DTO conversions
- DTO → Model conversions
- Command → Model conversions

**Recommendation**: Use AutoMapper or manual mapping extensions.

#### 5. MediatR Handlers (Not Implemented)
MediatR is installed but no handlers exist for:
- ❌ Command handlers
- ❌ Query handlers
- ❌ Pipeline behaviors (validation, logging)

## Detailed Gap Analysis by Feature Area

### 1. Workout Plan Management

**Current State**: Mock data in `WorkoutPlansController`

**Missing Business Logic**:
```
❌ IWorkoutPlanService
   - GetWorkoutPlansAsync(userId, page, pageSize)
   - GetWorkoutPlanByIdAsync(planId, userId)
   - CreateWorkoutPlanAsync(command, userId)
   - UpdateWorkoutPlanAsync(planId, command, userId)
   - DeleteWorkoutPlanAsync(planId, userId)
   - CheckIfPlanIsLockedAsync(planId) // Active session check

❌ ITrainingDayService
   - GetTrainingDaysForPlanAsync(planId)
   - AddTrainingDayAsync(planId, command, userId)
   - UpdateTrainingDayAsync(dayId, command, userId)
   - DeleteTrainingDayAsync(dayId, userId)
   - ReorderTrainingDaysAsync(planId, orderedIds, userId)

❌ IPlanExerciseService
   - GetExercisesForDayAsync(dayId)
   - AddExerciseToDayAsync(dayId, exerciseId, order, userId)
   - RemoveExerciseFromDayAsync(planDayExerciseId, userId)
   - ReorderExercisesAsync(dayId, orderedIds, userId)
```

**Business Rules to Implement**:
1. Cannot edit/delete plan if active session exists
2. Automatically assign order when adding training days
3. Automatically assign order when adding exercises
4. Validate plan belongs to user before operations
5. Cascade deletes properly handled

### 2. Exercise Management

**Current State**: Mock data in `ExercisesController`

**Missing Business Logic**:
```
❌ IExerciseService
   - GetExercisesAsync(search, muscleGroupId, page, pageSize, userId)
   - GetExerciseByIdAsync(exerciseId)
   - CreateExerciseAsync(command, userId)
   - ValidateExerciseNameUniqueForUserAsync(name, userId)

❌ IMuscleGroupService
   - GetAllMuscleGroupsAsync()
   - GetMuscleGroupByIdAsync(id)
```

**Business Rules to Implement**:
1. Users can only create exercises (not update/delete global ones)
2. Exercise names must be unique per user
3. Global exercises (userId = null) visible to all
4. User-created exercises only visible to owner

### 3. Session Management

**Current State**: Mock data in `SessionsController`, `SessionExercisesController`

**Missing Business Logic**:
```
❌ ISessionService
   - StartSessionAsync(trainingDayId, userId)
   - GetSessionByIdAsync(sessionId, userId)
   - GetSessionHistoryAsync(page, pageSize, userId)
   - UpdateSessionNotesAsync(sessionId, notes, userId)
   - FinishSessionAsync(sessionId, notes, userId)

❌ ISessionExerciseService
   - GetExerciseForSessionAsync(sessionExerciseId, userId)
   - UpdateSessionExerciseAsync(sessionExerciseId, command, userId)
   - MarkExerciseAsSkippedAsync(sessionExerciseId, userId)

❌ IExerciseSetService
   - AddSetAsync(sessionExerciseId, setDto, userId)
   - UpdateSetAsync(setId, setDto, userId)
   - DeleteSetAsync(setId, userId)
   - AssignOrderToSetsAsync(sessionExerciseId)

❌ IExerciseHistoryService
   - GetLastPerformanceAsync(exerciseId, userId)
   - GetExerciseHistoryAsync(exerciseId, userId, count)
```

**Business Rules to Implement**:
1. Session creation copies exercises from training day plan
2. Cannot start session if one already active for user
3. Sets automatically ordered
4. Skipped exercises have no sets
5. EndTime automatically set when finishing session
6. Previous performance lookup excludes skipped exercises

### 4. Authorization & Security

**Missing Implementation**:
```
❌ User Context Validation
   - All service methods must validate user ownership
   - Use IUserContextService from API layer
   - Throw UnauthorizedAccessException if mismatch

❌ RLS Policy Enforcement
   - Supabase queries automatically enforce RLS
   - Service layer provides additional checks for clarity
   - Better error messages than database errors
```

### 5. Error Handling

**Missing Implementation**:
```
❌ Custom Exceptions
   - NotFoundException
   - BusinessRuleViolationException
   - UnauthorizedAccessException
   - ValidationException

❌ Error Translation
   - Catch Postgrest exceptions
   - Translate to domain exceptions
   - Provide user-friendly messages
   - Log technical details
```

### 6. Validation

**Missing Implementation**:
```
❌ FluentValidation Setup
   - Install FluentValidation package
   - Create validators for all commands
   - Register validators in DI
   - Integrate with MediatR pipeline

❌ Required Validators:
   - CreateWorkoutPlanCommandValidator
     • Name required, max length
     • At least one training day
   
   - CreateExerciseCommandValidator
     • Name required, max length
     • Valid muscle group ID
   
   - UpdateSessionExerciseCommandValidator
     • Valid sets data
     • If skipped, sets should be empty
   
   - SetDataValidator
     • Weight >= 0
     • Reps >= 0
     • Order >= 0
```

## Architecture Decision: MediatR vs Direct Services

**Current Setup**: MediatR is installed but not used.

**Option 1: Use MediatR (Recommended in business-logic-plan.md)**
- ✅ Separation of concerns
- ✅ Pipeline behaviors (validation, logging)
- ✅ Easy to add cross-cutting concerns
- ❌ More initial setup
- ❌ Steeper learning curve

**Option 2: Direct Service Pattern (Currently in API)**
- ✅ Simpler, more straightforward
- ✅ Familiar pattern
- ✅ Less boilerplate
- ❌ Harder to add cross-cutting concerns
- ❌ Validation scattered

**Recommendation**: Since MediatR is already referenced, implement it properly with:
1. Command/Query handlers
2. Validation pipeline behavior
3. Logging pipeline behavior
4. Exception handling pipeline behavior

## Implementation Priority

### Phase 1: Core Services (P0 - Required for MVP)
1. **IExerciseService** + Implementation
   - GetExercisesAsync (needed by UI)
   - GetExerciseByIdAsync (needed by session page)
   - CreateExerciseAsync
   - GetLastPerformanceAsync (for showing previous performance)

2. **IMuscleGroupService** + Implementation
   - GetAllMuscleGroupsAsync (needed by dialogs)

3. **IWorkoutPlanService** + Implementation
   - GetWorkoutPlansAsync (home page)
   - GetWorkoutPlanByIdAsync (plan detail page)
   - CreateWorkoutPlanAsync
   - UpdateWorkoutPlanAsync
   - DeleteWorkoutPlanAsync

4. **ISessionService** + Implementation
   - StartSessionAsync (workout session)
   - UpdateSessionExerciseAsync (save sets)
   - FinishSessionAsync
   - GetSessionHistoryAsync (history page)
   - GetSessionByIdAsync (session summary)

### Phase 2: Advanced Features (P1)
5. **ITrainingDayService** + Implementation
   - AddTrainingDayAsync
   - UpdateTrainingDayAsync
   - DeleteTrainingDayAsync
   - ReorderTrainingDaysAsync

6. **IPlanExerciseService** + Implementation
   - AddExerciseToDayAsync
   - RemoveExerciseFromDayAsync
   - ReorderExercisesAsync

### Phase 3: Polish (P2)
7. **Validators** - FluentValidation setup
8. **MediatR Handlers** - If using CQRS pattern
9. **AutoMapper** - If using mapping library
10. **Custom Exceptions** - Better error handling

## Recommended Implementation Approach

### Step 1: Setup Infrastructure
```bash
# Install required packages
cd WorkoutManager.BusinessLogic
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
# MediatR already installed

# For mapping (optional)
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### Step 2: Create Service Interfaces
Create all service interfaces in `Services/Interfaces/` folder.

### Step 3: Implement Core Services
Start with Exercise and MuscleGroup services (simplest).
Then WorkoutPlan, then Session services (most complex).

### Step 4: Update API Controllers
Replace mock data with service calls.
Add proper error handling.
Add [Authorize] attributes.

### Step 5: Add Validation
Create FluentValidation validators.
Add validation to service methods.

### Step 6: Testing
Test each service with real Supabase connection.
Verify RLS policies work correctly.

## File Structure Recommendation

```
WorkoutManager.BusinessLogic/
├── Commands/
│   ├── (existing command files)
│   └── Handlers/         # NEW - MediatR handlers
│       ├── CreateWorkoutPlanHandler.cs
│       ├── UpdateSessionExerciseHandler.cs
│       └── ...
├── DTOs/
│   └── (existing DTO files - complete)
├── Services/
│   ├── Interfaces/       # NEW - all service interfaces
│   │   ├── IWorkoutPlanService.cs
│   │   ├── IExerciseService.cs
│   │   ├── ISessionService.cs
│   │   └── ...
│   └── Implementations/  # NEW - all service implementations
│       ├── WorkoutPlanService.cs
│       ├── ExerciseService.cs
│       ├── SessionService.cs
│       └── ...
├── Validators/           # NEW - FluentValidation validators
│   ├── CreateWorkoutPlanCommandValidator.cs
│   ├── CreateExerciseCommandValidator.cs
│   └── ...
├── Exceptions/           # NEW - custom exceptions
│   ├── NotFoundException.cs
│   ├── BusinessRuleViolationException.cs
│   └── ...
├── Mappings/             # NEW - AutoMapper profiles (optional)
│   ├── WorkoutPlanProfile.cs
│   ├── SessionProfile.cs
│   └── ...
└── Extensions/           # NEW - helper extensions
    ├── ServiceCollectionExtensions.cs
    └── ...
```

## Summary

**Overall Status**: ~15% Complete

- ✅ Data models: 100%
- ✅ DTOs: 95%
- ⚠️ Commands: 40% (exist but not used)
- ❌ Service interfaces: 0%
- ❌ Service implementations: 0%
- ❌ Validators: 0%
- ❌ Handlers: 0%
- ❌ Mappers: 0%

**Estimated Work**:
- ~8-10 service interfaces
- ~8-10 service implementations (each with 5-10 methods)
- ~6-8 validators
- Error handling infrastructure
- Testing

**Critical Path**: Services must be implemented before API can move away from mock data. This is the single biggest blocker to a functional MVP.

