# Business Logic Implementation Plan

## Executive Summary

**Current Status**: Business Logic layer is ~15% complete
- ✅ Data models exist
- ✅ DTOs mostly complete
- ❌ **No service layer exists** (critical blocker)
- ❌ No validation
- ❌ No business rules enforcement

**Goal**: Implement complete service layer with Supabase integration to replace mock data in API controllers.

## Implementation Phases

### Phase 0: Prerequisites & Setup

#### 0.1 Install Required Packages
```bash
cd WorkoutManager.BusinessLogic

# FluentValidation for validation
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# Supabase SDK (if not already added)
dotnet add package Supabase

# Optional: AutoMapper for DTO mapping
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

#### 0.2 Create Folder Structure
```
WorkoutManager.BusinessLogic/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Validators/
├── Exceptions/
└── Extensions/
```

---

## Phase 1: Foundation Services (Days 1-2)

### 1.1 Create Custom Exceptions

**File**: `WorkoutManager.BusinessLogic/Exceptions/NotFoundException.cs`
```csharp
namespace WorkoutManager.BusinessLogic.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}
```

**File**: `WorkoutManager.BusinessLogic/Exceptions/BusinessRuleViolationException.cs`
```csharp
namespace WorkoutManager.BusinessLogic.Exceptions;

public class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message) : base(message) { }
}
```

**File**: `WorkoutManager.BusinessLogic/Exceptions/UnauthorizedAccessException.cs`
```csharp
namespace WorkoutManager.BusinessLogic.Exceptions;

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message = "Unauthorized access to resource")
        : base(message) { }
}
```

### 1.2 IMuscleGroupService (Simplest - Start Here)

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/IMuscleGroupService.cs`
```csharp
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IMuscleGroupService
{
    Task<PaginatedList<MuscleGroupDto>> GetAllMuscleGroupsAsync(int page = 1, int pageSize = 20);
    Task<MuscleGroupDto?> GetMuscleGroupByIdAsync(int id);
}
```

**File**: `WorkoutManager.BusinessLogic/Services/Implementations/MuscleGroupService.cs`
```csharp
using Supabase;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly Client _supabaseClient;

    public MuscleGroupService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<PaginatedList<MuscleGroupDto>> GetAllMuscleGroupsAsync(int page = 1, int pageSize = 20)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        var response = await _supabaseClient
            .From<MuscleGroup>()
            .Range(from, to)
            .Get();

        var count = response.Models.Count; // Supabase returns count in headers

        var dtos = response.Models.Select(mg => new MuscleGroupDto
        {
            Id = (int)mg.Id,
            Name = mg.Name
        }).ToList();

        return new PaginatedList<MuscleGroupDto>
        {
            Data = dtos,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = count
            }
        };
    }

    public async Task<MuscleGroupDto?> GetMuscleGroupByIdAsync(int id)
    {
        var response = await _supabaseClient
            .From<MuscleGroup>()
            .Where(mg => mg.Id == id)
            .Single();

        if (response == null) return null;

        return new MuscleGroupDto
        {
            Id = (int)response.Id,
            Name = response.Name
        };
    }
}
```

### 1.3 IExerciseService

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/IExerciseService.cs`
```csharp
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IExerciseService
{
    Task<PaginatedList<ExerciseDto>> GetExercisesAsync(
        Guid userId,
        string? search = null,
        int? muscleGroupId = null,
        int page = 1,
        int pageSize = 20);
    
    Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId);
    
    Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId);
    
    Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(int exerciseId, Guid userId);
}
```

**File**: `WorkoutManager.BusinessLogic/Services/Implementations/ExerciseService.cs`
```csharp
using Supabase;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class ExerciseService : IExerciseService
{
    private readonly Client _supabaseClient;

    public ExerciseService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<PaginatedList<ExerciseDto>> GetExercisesAsync(
        Guid userId,
        string? search = null,
        int? muscleGroupId = null,
        int page = 1,
        int pageSize = 20)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        var query = _supabaseClient
            .From<Exercise>()
            .Where(e => e.UserId == null || e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.ToLower().Contains(search.ToLower()));
        }

        if (muscleGroupId.HasValue)
        {
            query = query.Where(e => e.MuscleGroupId == muscleGroupId.Value);
        }

        var response = await query.Range(from, to).Get();

        var dtos = response.Models.Select(e => new ExerciseDto
        {
            Id = (int)e.Id,
            UserId = e.UserId,
            MuscleGroupId = (int)e.MuscleGroupId,
            Name = e.Name
        }).ToList();

        return new PaginatedList<ExerciseDto>
        {
            Data = dtos,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = response.Models.Count
            }
        };
    }

    public async Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId)
    {
        var response = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.Id == exerciseId)
            .Single();

        if (response == null) return null;

        return new ExerciseDto
        {
            Id = (int)response.Id,
            UserId = response.UserId,
            MuscleGroupId = (int)response.MuscleGroupId,
            Name = response.Name
        };
    }

    public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId)
    {
        // Check for duplicate name
        var existing = await _supabaseClient
            .From<Exercise>()
            .Where(e => e.UserId == userId && e.Name.ToLower() == dto.Name.ToLower())
            .Get();

        if (existing.Models.Any())
        {
            throw new BusinessRuleViolationException($"An exercise named '{dto.Name}' already exists.");
        }

        var exercise = new Exercise
        {
            UserId = userId,
            MuscleGroupId = dto.MuscleGroupId,
            Name = dto.Name
        };

        var response = await _supabaseClient
            .From<Exercise>()
            .Insert(exercise);

        var created = response.Models.First();

        return new ExerciseDto
        {
            Id = (int)created.Id,
            UserId = created.UserId,
            MuscleGroupId = (int)created.MuscleGroupId,
            Name = created.Name
        };
    }

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(int exerciseId, Guid userId)
    {
        // Query for most recent session with this exercise (not skipped)
        var sessionExercises = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.ExerciseId == exerciseId && !se.Skipped)
            .Order("session.start_time", Ordering.Descending)
            .Limit(1)
            .Get();

        if (!sessionExercises.Models.Any()) return null;

        var sessionExercise = sessionExercises.Models.First();

        // Get the sets for this session exercise
        var sets = await _supabaseClient
            .From<ExerciseSet>()
            .Where(s => s.SessionExerciseId == sessionExercise.Id)
            .Order("order", Ordering.Ascending)
            .Get();

        // Get session date
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionExercise.SessionId)
            .Single();

        return new PreviousExercisePerformanceDto
        {
            SessionDate = session.StartTime,
            Notes = sessionExercise.Notes,
            Sets = sets.Models.Select(s => new PreviousExerciseSetDto
            {
                Weight = s.Weight,
                Reps = s.Reps,
                IsFailure = s.IsFailure
            }).ToList()
        };
    }
}
```

---

## Phase 2: Workout Plan Services (Days 3-4)

### 2.1 IWorkoutPlanService

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/IWorkoutPlanService.cs`
```csharp
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IWorkoutPlanService
{
    Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<WorkoutPlanDetailDto> GetWorkoutPlanByIdAsync(int planId, Guid userId);
    Task<CreatedWorkoutPlanDto> CreateWorkoutPlanAsync(CreateWorkoutPlanCommand command, Guid userId);
    Task UpdateWorkoutPlanAsync(int planId, UpdateWorkoutPlanPayload payload, Guid userId);
    Task DeleteWorkoutPlanAsync(int planId, Guid userId);
    Task<bool> IsPlanLockedAsync(int planId, Guid userId);
}
```

**Implementation Notes**:
- CheckIfPlanIsLocked: Query sessions table for active session (end_time IS NULL)
- CreateWorkoutPlan: Insert plan, then insert training days with auto-assigned order
- UpdateWorkoutPlan: Check lock status first, update name, reorder training days
- DeleteWorkoutPlan: Check lock status first, then delete (cascade handled by DB)

### 2.2 ITrainingDayService

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/ITrainingDayService.cs`
```csharp
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ITrainingDayService
{
    Task<TrainingDayDto> AddTrainingDayAsync(int planId, string name, Guid userId);
    Task UpdateTrainingDayAsync(int dayId, string name, Guid userId);
    Task DeleteTrainingDayAsync(int dayId, Guid userId);
    Task ReorderTrainingDaysAsync(int planId, List<UpdateTrainingDayOrderCommand> orderedDays, Guid userId);
}
```

### 2.3 IPlanExerciseService

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/IPlanExerciseService.cs`
```csharp
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface IPlanExerciseService
{
    Task<CreatedPlanDayExerciseDto> AddExerciseToDayAsync(
        int planId,
        int dayId,
        AddExerciseToTrainingDayCommand command,
        Guid userId);
    
    Task RemoveExerciseFromDayAsync(int planId, int dayId, int planDayExerciseId, Guid userId);
    
    Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises, Guid userId);
}
```

---

## Phase 3: Session Services (Days 5-7)

### 3.1 ISessionService

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/ISessionService.cs`
```csharp
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionService
{
    Task<SessionDetailsDto> StartSessionAsync(int trainingDayId, Guid userId);
    Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<SessionDetailsDto> GetSessionByIdAsync(int sessionId, Guid userId);
    Task UpdateSessionNotesAsync(int sessionId, string? notes, Guid userId);
    Task FinishSessionAsync(int sessionId, string? notes, Guid userId);
    Task<bool> HasActiveSessionAsync(Guid userId);
}
```

**Key Implementation Details**:
```csharp
// StartSessionAsync logic:
1. Check if user has active session (throw BusinessRuleViolationException)
2. Get training day with exercises
3. Create Session record
4. Copy exercises from plan to SessionExercise records
5. Return SessionDetailsDto with empty sets

// GetSessionHistoryAsync logic:
1. Query sessions with plan and training day joins
2. Map to SessionSummaryDto with plan/day names
3. Order by start_time DESC

// FinishSessionAsync logic:
1. Verify session belongs to user
2. Update notes and end_time
3. Return updated session
```

### 3.2 ISessionExerciseService

**File**: `WorkoutManager.BusinessLogic/Services/Interfaces/ISessionExerciseService.cs`
```csharp
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionExerciseService
{
    Task<SessionExerciseDetailsDto> UpdateSessionExerciseAsync(
        int sessionId,
        int sessionExerciseId,
        UpdateSessionExerciseCommand command,
        Guid userId);
    
    Task MarkAsSkippedAsync(int sessionExerciseId, Guid userId);
}
```

**Key Implementation Details**:
```csharp
// UpdateSessionExerciseAsync logic:
1. Verify session belongs to user
2. Update notes, skipped flag
3. Delete existing sets
4. If not skipped, insert new sets with auto-assigned order
5. Return updated SessionExerciseDetailsDto
```

---

## Phase 4: Validation (Day 8)

### 4.1 FluentValidation Validators

**File**: `WorkoutManager.BusinessLogic/Validators/CreateWorkoutPlanCommandValidator.cs`
```csharp
using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class CreateWorkoutPlanCommandValidator : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(200).WithMessage("Plan name cannot exceed 200 characters");

        RuleFor(x => x.TrainingDays)
            .NotEmpty().WithMessage("At least one training day is required");

        RuleForEach(x => x.TrainingDays)
            .SetValidator(new CreateTrainingDayCommandValidator());
    }
}

public class CreateTrainingDayCommandValidator : AbstractValidator<CreateTrainingDayCommand>
{
    public CreateTrainingDayCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Training day name is required")
            .MaximumLength(200).WithMessage("Training day name cannot exceed 200 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}
```

**File**: `WorkoutManager.BusinessLogic/Validators/CreateExerciseCommandValidator.cs`
```csharp
using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class CreateExerciseCommandValidator : AbstractValidator<CreateExerciseCommand>
{
    public CreateExerciseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exercise name is required")
            .MaximumLength(200).WithMessage("Exercise name cannot exceed 200 characters");

        RuleFor(x => x.MuscleGroupId)
            .GreaterThan(0).WithMessage("Valid muscle group must be selected");
    }
}
```

**File**: `WorkoutManager.BusinessLogic/Validators/UpdateSessionExerciseCommandValidator.cs`
```csharp
using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class UpdateSessionExerciseCommandValidator : AbstractValidator<UpdateSessionExerciseCommand>
{
    public UpdateSessionExerciseCommandValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        When(x => !x.Skipped, () =>
        {
            RuleFor(x => x.Sets)
                .NotEmpty().WithMessage("At least one set is required for non-skipped exercises");
        });

        RuleForEach(x => x.Sets)
            .SetValidator(new UpdateExerciseSetDtoValidator());
    }
}

public class UpdateExerciseSetDtoValidator : AbstractValidator<UpdateExerciseSetDto>
{
    public UpdateExerciseSetDtoValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be non-negative");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be greater than 0");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}
```

### 4.2 Validation in Services

Add validation calls in service methods:
```csharp
public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId)
{
    // Validate
    var validator = new CreateExerciseCommandValidator();
    var command = new CreateExerciseCommand { Name = dto.Name, MuscleGroupId = dto.MuscleGroupId };
    var validationResult = await validator.ValidateAsync(command);
    
    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }
    
    // Business logic...
}
```

---

## Phase 5: API Controller Integration (Days 9-10)

### 5.1 Update API Program.cs

Register services:
```csharp
// Register business logic services
builder.Services.AddScoped<IMuscleGroupService, MuscleGroupService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<ITrainingDayService, TrainingDayService>();
builder.Services.AddScoped<IPlanExerciseService, PlanExerciseService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionExerciseService, SessionExerciseService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateWorkoutPlanCommandValidator>();
```

### 5.2 Update Controllers

**Example**: Update `ExercisesController.cs`
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;
    private readonly IUserContextService _userContext;

    public ExercisesController(IExerciseService exerciseService, IUserContextService userContext)
    {
        _exerciseService = exerciseService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ExerciseDto>>> GetExercises(
        [FromQuery] string? search,
        [FromQuery] int? muscleGroupId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var result = await _exerciseService.GetExercisesAsync(userId, search, muscleGroupId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetExerciseById(int id)
    {
        try
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            if (exercise == null) return NotFound();
            return Ok(exercise);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto dto)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var exercise = await _exerciseService.CreateExerciseAsync(dto, userId);
            return CreatedAtAction(nameof(GetExerciseById), new { id = exercise.Id }, exercise);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{exerciseId}/previous-session")]
    public async Task<ActionResult<PreviousExercisePerformanceDto>> GetPreviousPerformance(int exerciseId)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();
            var performance = await _exerciseService.GetLastPerformanceAsync(exerciseId, userId);
            
            if (performance == null) return NotFound("No previous performance found");
            
            return Ok(performance);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
```

Repeat this pattern for all controllers.

---

## Phase 6: Testing & Refinement (Days 11-12)

### 6.1 Integration Testing
- Test each service with real Supabase connection
- Verify RLS policies work correctly
- Test error handling
- Test validation

### 6.2 End-to-End Testing
- Test complete user flows
- Create plan → Add exercises → Start session → Complete session → View history
- Verify data consistency

---

## Implementation Checklist

### Setup
- [ ] Install FluentValidation packages
- [ ] Install Supabase package (if not done)
- [ ] Create folder structure (Services/Interfaces, Services/Implementations, Validators, Exceptions)

### Exceptions
- [ ] Create NotFoundException
- [ ] Create BusinessRuleViolationException
- [ ] Create UnauthorizedAccessException

### Services - Phase 1
- [ ] IMuscleGroupService + Implementation
- [ ] IExerciseService + Implementation
- [ ] Test both services

### Services - Phase 2
- [ ] IWorkoutPlanService + Implementation
- [ ] ITrainingDayService + Implementation
- [ ] IPlanExerciseService + Implementation
- [ ] Test workout plan services

### Services - Phase 3
- [ ] ISessionService + Implementation
- [ ] ISessionExerciseService + Implementation
- [ ] Test session services

### Validation
- [ ] CreateWorkoutPlanCommandValidator
- [ ] CreateExerciseCommandValidator
- [ ] UpdateSessionExerciseCommandValidator
- [ ] Integrate validators into services

### API Integration
- [ ] Update Program.cs - register services
- [ ] Update ExercisesController
- [ ] Update MuscleGroupsController
- [ ] Update WorkoutPlansController
- [ ] Update SessionsController
- [ ] Update SessionExercisesController
- [ ] Update PlanDayExercisesController
- [ ] Add [Authorize] to all controllers
- [ ] Remove all mock data

### Testing
- [ ] Test exercise creation and retrieval
- [ ] Test workout plan CRUD
- [ ] Test session workflow
- [ ] Test validation rules
- [ ] Test error handling
- [ ] End-to-end user flow testing

---

## Estimated Timeline

- **Days 1-2**: Foundation (Exceptions, MuscleGroup, Exercise services)
- **Days 3-4**: Workout Plan services
- **Days 5-7**: Session services
- **Day 8**: Validation
- **Days 9-10**: API integration
- **Days 11-12**: Testing & bug fixes

**Total**: ~12 working days for 1 developer

---

## Success Criteria

- [ ] All API endpoints use services (no mock data)
- [ ] All services have proper error handling
- [ ] All commands have validation
- [ ] All database operations use Supabase SDK
- [ ] RLS policies enforced
- [ ] User can complete full workflow: Register → Create Plan → Start Session → Complete Session → View History
- [ ] No critical bugs in main user flows

