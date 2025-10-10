<!-- 16d4153e-20e4-4931-b46a-f61de7dc3116 2d15fb4c-e35a-43e3-907b-264b50d1642a -->
# Business Logic Implementation Plan: 10xWorkoutManager

## 1. Business Logic Overview

This plan details the creation of a dedicated business logic layer for the 10xWorkoutManager application. This layer will reside in a separate C# project and act as an intermediary between the Blazor UI and the Supabase backend. Its purpose is to encapsulate all business rules, data validation, and data access logic, creating a clean, maintainable, and scalable architecture. The layer will be composed of services, each handling a specific domain resource as outlined in the business logic specification.

## 2. Used Types

The implementation will introduce several new types, primarily Data Transfer Objects (DTOs), to ensure a clean separation between the data model and the presentation layer.

-   **Project**: A new class library project, `WorkoutManager.Business`, will be created to house all the types and logic described below.
-   **DTOs**: All DTOs listed in the business logic specification will be created within the `WorkoutManager.Business/DTOs` directory. Examples include:
    -   `WorkoutPlanDto`, `WorkoutPlanSummaryDto`
    -   `TrainingDayDto`
    -   `ExerciseDto`, `CreateExerciseDto`
    -   `SessionDto`, `SessionSummaryDto`
    -   `ExerciseSetDto`
-   **Validation Models**: For each DTO that requires validation (e.g., `CreateExerciseDto`), a corresponding validator class will be created using the `FluentValidation` library (e.g., `CreateExerciseDtoValidator`).
-   **Mapping Profiles**: `AutoMapper` profiles will be created to define the mappings between the data entities in `WorkoutManager.Data/Models` and the DTOs.

## 3. Data Flow

The data will flow from the UI components to the database through the new service layer in a structured manner.

1.  **UI Interaction**: A Blazor component (e.g., `WorkoutPlansPage.razor`) will inject and call a method on a service interface (e.g., `IWorkoutPlanService.CreateWorkoutPlan(planDto)`).
2.  **Service Execution**: The service implementation (`WorkoutPlanService`) receives the request.
3.  **Validation**: It first uses the corresponding `FluentValidation` validator to check the input DTO. If validation fails, it throws a `ValidationException`.
4.  **Mapping**: If validation succeeds, it uses `AutoMapper` to map the input DTO to a `WorkoutPlan` data entity.
5.  **Database Operation**: The service uses the injected `Supabase.Client` to perform the required database operation (e.g., insert the new `WorkoutPlan` entity).
6.  **Response Handling**: The service catches any `PostgrestException` from Supabase, logs it, and translates it into an application-specific exception or a result object.
7.  **Return Value**: If successful, it maps the result from the database back to a DTO and returns it to the Blazor component.
8.  **UI Update**: The Blazor component receives the DTO or the exception and updates the UI accordingly.

## 4. Security Considerations

Security is paramount and will be addressed through a multi-layered approach.

-   **Authorization**: The primary authorization mechanism is PostgreSQL's Row-Level Security (RLS), as defined in the database plan. The business services will rely on RLS to enforce data access rules. It is critical that the `Supabase.Client` is configured as a scoped service and initialized with the authenticated user's token on each request.
-   **Mass Assignment**: The use of DTOs inherently protects against mass assignment vulnerabilities. Only the properties defined in the DTOs can be mapped and updated, preventing malicious users from modifying sensitive fields (e.g., `UserId`).
-   **Input Validation**: All data coming from the client will be rigorously validated by `FluentValidation` to prevent invalid data from reaching the database, mitigating risks like SQL injection (already handled by the Supabase client) and data corruption.

## 5. Error Handling

A robust error handling strategy will be implemented to provide clear feedback to users and developers.

-   **Exception Translation**: Service methods will be wrapped in `try-catch` blocks. Low-level `PostgrestException`s from the Supabase client will be caught and translated into specific, custom exceptions.
-   **Custom Exceptions**: A set of custom exceptions will be created:
    -   `ValidationException`: Thrown when input DTO validation fails.
    -   `NotFoundException`: Thrown when a requested resource does not exist.
    -   `DuplicateEntryException`: Thrown on unique constraint violations.
    -   `DatabaseException`: For other general database errors.
-   **Logging**: The built-in `ILogger` will be used to log the original exception details for debugging purposes before the translated exception is thrown.
-   **UI Feedback**: The UI layer will be responsible for catching these custom exceptions and displaying appropriate, user-friendly error messages.

## 6. Performance Considerations

To ensure the application is responsive, the following performance strategies will be considered.

-   **Pagination**: For any requests that may return large lists of data (e.g., `GetWorkoutPlans`, `GetSessionHistory`), pagination will be implemented. Service methods will accept `page` and `pageSize` parameters, which will be translated to Supabase's `.Range()` query modifier.
-   **Data Projection**: When fetching lists for summary views (e.g., `WorkoutPlanSummaryDto`), the services will use Supabase's `.Select()` method to query only the necessary columns, reducing the data payload and improving query speed.
-   **Caching**: For data that changes infrequently, such as the list of `MuscleGroup`s, a caching layer using `IMemoryCache` will be implemented within the `IExerciseService` to minimize redundant database calls.

## 7. Implementation Steps

1.  **Project Setup**:

    -   Create a new .NET Class Library project named `WorkoutManager.Business`.
    -   Add project references: `WorkoutManager.Business` -> `WorkoutManager.Data`, and `WorkoutManager.Web` -> `WorkoutManager.Business`.

2.  **Install Dependencies**: Add the following NuGet packages to `WorkoutManager.Business`:

    -   `supabase-csharp`
    -   `AutoMapper`
    -   `AutoMapper.Extensions.Microsoft.DependencyInjection`
    -   `FluentValidation`
    -   `FluentValidation.DependencyInjectionExtensions`

3.  **Define Core Types**:

    -   Create all DTO classes inside a `WorkoutManager.Business/DTOs` folder.
    -   Create all service interfaces (`IWorkoutPlanService`, etc.) inside a `WorkoutManager.Business/Interfaces` folder.
    -   Create the custom exception classes (`ValidationException`, etc.) inside `WorkoutManager.Business/Exceptions`.

4.  **Configuration**:

    -   In `WorkoutManager.Web/Program.cs`, configure the DI container.
    -   Register the Supabase client.
    -   Register all services with a scoped lifetime (e.g., `builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();`).
    -   Register AutoMapper (`builder.Services.AddAutoMapper(...)`).
    -   Register FluentValidation (`builder.Services.AddValidatorsFromAssembly(...)`).

5.  **Implement Mappers and Validators**:

    -   Create AutoMapper profiles in `WorkoutManager.Business/Mappers` to define entity-to-DTO mappings.
    -   Create `FluentValidation` validator classes in `WorkoutManager.Business/Validators` for all relevant DTOs.

6.  **Implement Services**:

    -   Create a `WorkoutManager.Business/Services` folder.
    -   Implement each service one by one. Inject dependencies (`Supabase.Client`, `IMapper`, `ILogger`).
    -   Implement the data flow logic: validate input, map DTOs, call Supabase, handle errors, and map results back to DTOs.

7.  **Integration and Testing**:

    -   As each service is implemented, integrate it into the corresponding Blazor components in the `WorkoutManager.Web` project.
    -   Thoroughly test each service method's "happy path" and error scenarios to ensure correctness and robustness.