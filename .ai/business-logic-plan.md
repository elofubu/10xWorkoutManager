# Business Layer Plan

This document outlines the business layer for the 10xWorkoutManager application. This layer will be responsible for encapsulating business logic, data validation, and communication with the Supabase backend. It will be composed of services that will be injected into the Blazor UI components.

## 1. Resources

The main resources of the application are directly mapped from the database tables:

-   **WorkoutPlan**: Corresponds to the `workout_plans` table. Represents a user's workout plan.
-   **TrainingDay**: Corresponds to the `training_days` table. A component of a workout plan.
-   **PlanDayExercise**: Corresponds to the `plan_day_exercises` table. An exercise within a training day.
-   **Exercise**: Corresponds to the `exercises` table. A predefined or user-created exercise.
-   **MuscleGroup**: Corresponds to the `muscle_groups` table.
-   **Session**: Corresponds to the `sessions` table. A record of a workout session.
-   **SessionExercise**: Corresponds to the `session_exercises` table. An exercise performed during a session.
-   **ExerciseSet**: Corresponds to the `exercise_sets` table. A set of repetitions and weight for a session exercise.

## 2. Data Transfer Objects (DTOs)

To decouple the UI from the database schema, services will use DTOs for input and output. This improves security and performance by ensuring only necessary data is transferred. A mapping library like AutoMapper will be used to convert between database entities and DTOs.

-   **WorkoutPlanDto**: Contains all details of a workout plan.
-   **WorkoutPlanSummaryDto**: A lightweight version for lists, containing `Id`, `Name`, and maybe `NumberOfDays`.
-   **TrainingDayDto**: Represents a single day in a plan.
-   **PlanDayExerciseDto**: Represents an exercise within a training day, likely containing exercise details.
-   **ExerciseDto**: Represents a single exercise with its muscle group.
-   **CreateExerciseDto**: DTO for creating a new exercise.
-   **MuscleGroupDto**: Represents a muscle group.
-   **SessionDto**: Contains full details of a past session, including all exercises and sets. Also includes `PlanId`, `TrainingDayId`, `PlanName`, and `TrainingDayName`.
-   **SessionSummaryDto**: A lightweight version for the history list, with `Id`, `StartTime`, `EndTime`, `PlanId`, `TrainingDayId`, `PlanName`, and `TrainingDayName`.
-   **ExerciseSetDto**: Represents a single set performed by the user.
-   **LastExercisePerformanceDto**: A specific DTO to show the last performance of an exercise during an active session.

## 3. Interfaces and Services

The business logic will be organized into several services, each with a specific responsibility.

### IAuthService

-   **Description**: Handles user authentication and management.
-   **Methods**:
    -   `Task<User> Register(string email, string password)`
    -   `Task<Session> Login(string email, string password)`
    -   `Task Logout()`
    -   `Task<bool> ResetPassword(string email)`
    -   `User GetCurrentUser()`
-   **Input/Output**: Uses Supabase's `User` and `Session` models.

### IWorkoutPlanService

-   **Description**: Manages CRUD operations for `WorkoutPlan` resources.
-   **Methods**:
    -   `Task<IEnumerable<WorkoutPlanSummaryDto>> GetWorkoutPlans(int page, int pageSize)`
    -   `Task<WorkoutPlanDto> GetWorkoutPlanById(long id)`
    -   `Task<WorkoutPlanDto> CreateWorkoutPlan(WorkoutPlanDto newPlanDto)`
    -   `Task<WorkoutPlanDto> UpdateWorkoutPlan(WorkoutPlanDto updatedPlanDto)`
    -   `Task DeleteWorkoutPlan(long id)`
-   **Input/Output**: `WorkoutPlanDto`, `WorkoutPlanSummaryDto`

### ITrainingDayService

-   **Description**: Manages `TrainingDay` resources within a `WorkoutPlan`.
-   **Methods**:
    -   `Task<IEnumerable<TrainingDayDto>> GetTrainingDaysForPlan(long planId)`
    -   `Task<TrainingDayDto> AddTrainingDay(TrainingDayDto newDayDto)`
    -   `Task<TrainingDayDto> UpdateTrainingDay(TrainingDayDto updatedDayDto)`
    -   `Task DeleteTrainingDay(long id)`
    -   `Task ReorderTrainingDays(long planId, IEnumerable<long> orderedDayIds)`
-   **Input/Output**: `TrainingDayDto`

### IPlanExerciseService

-   **Description**: Manages exercises within a `TrainingDay`.
-   **Methods**:
    -   `Task<IEnumerable<PlanDayExerciseDto>> GetExercisesForDay(long trainingDayId)`
    -   `Task AddExerciseToDay(long trainingDayId, long exerciseId)`
    -   `Task RemoveExerciseFromDay(long planDayExerciseId)`
    -   `Task ReorderExercises(long trainingDayId, IEnumerable<long> orderedPlanDayExerciseIds)`
-   **Input/Output**: `PlanDayExerciseDto`

### IExerciseService

-   **Description**: Manages `Exercise` and `MuscleGroup` resources.
-   **Methods**:
    -   `Task<IEnumerable<ExerciseDto>> GetExercises(string searchTerm, int page, int pageSize)` (gets both predefined and user-owned)
    -   `Task<ExerciseDto> CreateExercise(CreateExerciseDto newExerciseDto)`
    -   `Task<ExerciseDto> UpdateExercise(ExerciseDto updatedExerciseDto)` (only for user-owned)
    -   `Task DeleteExercise(long id)` (only for user-owned)
    -   `Task<IEnumerable<MuscleGroupDto>> GetMuscleGroups()`
-   **Input/Output**: `ExerciseDto`, `CreateExerciseDto`, `MuscleGroupDto`

### ISessionService

-   **Description**: Manages workout `Session`s.
-   **Methods**:
    -   `Task<SessionDto> StartSession(long? trainingDayId)` - Starts a new session. If `trainingDayId` is provided, links the session to that specific training day and pre-populates exercises from that day. If null, creates an ad-hoc session.
    -   `Task<SessionDto> EndSession(long sessionId)`
    -   `Task<IEnumerable<SessionSummaryDto>> GetSessionHistory(int page, int pageSize)`
    -   `Task<SessionDto> GetSessionDetails(long sessionId)`
-   **Input/Output**: `SessionDto`, `SessionSummaryDto`
-   **Note**: Sessions track which specific training day was performed via `training_day_id`, enabling better history tracking and progression analysis.

### ISessionExerciseService & IExerciseSetService

-   **Description**: Manages exercises and sets within an active `Session`.
-   **Methods**:
    -   `Task<SessionExerciseDto> AddExerciseToSession(long sessionId, long exerciseId)`
    -   `Task<SessionExerciseDto> SkipExercise(long sessionExerciseId)`
    -   `Task<ExerciseSetDto> AddSet(long sessionExerciseId, ExerciseSetDto newSetDto)`
    -   `Task<ExerciseSetDto> UpdateSet(ExerciseSetDto updatedSetDto)`
    -   `Task DeleteSet(long setId)`
-   **Input/Output**: `SessionExerciseDto`, `ExerciseSetDto`

### IExerciseHistoryService

-   **Description**: Provides historical data about exercise performance.
-   **Methods**:
    -   `Task<LastExercisePerformanceDto> GetLastPerformance(long exerciseId)`
-   **Input/Output**: `LastExercisePerformanceDto`

## 4. Validation, Mapping, and Business Logic

-   **Validation**: All service methods that accept DTOs for creation or update will perform validation. This includes checking for required fields, value ranges (`>= 0`), and other business rules. A library like `FluentValidation` is recommended to define validation rules for each DTO.
-   **Mapping**: A library like **AutoMapper** will be used to handle the conversion between database entities and DTOs. This will keep the service logic clean and focused on business operations.
-   **Business Logic Implementation**:
    -   **Authorization**: Logic is primarily handled by PostgreSQL's Row Level Security (RLS) policies. Services operate on behalf of the authenticated user, and the database enforces access control. The services will not contain explicit user ID checks.
    -   **Data Integrity**: Services will ensure that operations are logical (e.g., you cannot add a set to a skipped exercise).
    -   **Error Handling**: Services will catch specific `PostgrestException` types from the `supabase-csharp` client and wrap them in custom, application-specific exceptions or return a `Result` object. This prevents leaking data access details to the UI and provides clear error information for creating user-friendly messages. For example, a unique constraint violation will be translated into a "This name is already taken" message in the UI.
    -   **Ordering**: For entities with an `order` column (`TrainingDay`, `PlanDayExercise`, etc.), the services will manage reordering logic, ensuring the `order` values are updated correctly when items are moved.
