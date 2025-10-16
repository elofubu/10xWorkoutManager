# Refactoring Plan: Relocating Repositories to the Data Layer

## 1. Objective

The primary goal of this refactoring is to improve the architecture of the solution by relocating the repository implementations from the `WorkoutManager.BusinessLogic` project to the `WorkoutManager.Data` project. This will enforce a clear Separation of Concerns (SoC), aligning the project with clean architecture principles where the data layer is solely responsible for data access logic.

## 2. The Challenge: Circular Dependencies

A direct move of the repository implementation files is not possible. It would create a circular dependency:
1.  `WorkoutManager.BusinessLogic` needs to reference `WorkoutManager.Data` to use the data models in its service and repository interface definitions.
2.  If we move the repository implementations to `WorkoutManager.Data`, it would need to reference `WorkoutManager.BusinessLogic` to implement the repository interfaces.

This `BusinessLogic` <-> `Data` circular reference is not allowed by the .NET project system.

## 3. The Solution: A New Domain Project

To solve this, we will introduce a new, central project, `WorkoutManager.Domain`. This project will contain the shared, core assets of our application: the data models and the repository interfaces. It will have no dependencies on any other project in the solution.

The new dependency flow will be:
- `WorkoutManager.BusinessLogic` -> `WorkoutManager.Domain`
- `WorkoutManager.Data` -> `WorkoutManager.Domain`

This resolves the circular dependency and correctly isolates the different layers of the application.

## 4. Implementation Steps

The refactoring will be executed in the following sequence:

### Step 1: Create the `WorkoutManager.Domain` Project
- Create a new .NET Standard or .NET 9.0 Class Library project named `WorkoutManager.Domain`.
- Add this project to the `WorkoutManager.sln` solution file.

### Step 2: Move Core Assets to the Domain Project
- **Move Data Models**: Relocate all model files from `WorkoutManager.Data/Models` to a new `WorkoutManager.Domain/Models` folder.
- **Move Repository Interfaces**: Relocate all repository interface files (e.g., `IWorkoutPlanRepository.cs`) from `WorkoutManager.BusinessLogic/Services/Interfaces` to a new `WorkoutManager.Domain/Repositories` folder.
- **Update Namespaces**: Adjust the namespaces in all moved files to reflect their new location (e.g., `namespace WorkoutManager.Domain.Models;`).

### Step 3: Move Repository Implementations to the Data Project
- **Move Implementations**: Relocate all repository implementation files (e.g., `WorkoutPlanRepository.cs`) from `WorkoutManager.BusinessLogic/Services/Implementations` to a new `WorkoutManager.Data/Repositories` folder.
- **Update Namespaces**: Adjust the namespaces in these files to `namespace WorkoutManager.Data.Repositories;`.

### Step 4: Update Project References
- In `WorkoutManager.Data.csproj`:
  - Add a project reference to `WorkoutManager.Domain`.
  - Ensure it has the `Supabase` package reference, as the repositories depend on it.
- In `WorkoutManager.BusinessLogic.csproj`:
  - Add a project reference to `WorkoutManager.Domain`.
  - **Remove** the existing project reference to `WorkoutManager.Data`.
- In `WorkoutManager.Api.csproj`, `WorkoutManager.Api.Tests.csproj`, and `WorkoutManager.BusinessLogic.Tests.csproj`:
  - Review and update project references to ensure they can still access all required types. They will likely need a reference to the new `WorkoutManager.Domain` project.

### Step 5: Resolve Build Errors
- After moving files and updating references, the solution will have many build errors.
- Systematically go through each project (`Data`, `BusinessLogic`, `Api`, and all test projects) and update the `using` statements at the top of the files to point to the new namespaces (e.g., `using WorkoutManager.Domain.Models;`, `using WorkoutManager.Domain.Repositories;`).
- Compile the solution and fix any remaining issues until the build succeeds.

## 5. Expected Outcome

Upon completion, the solution will have a cleaner, more maintainable architecture:
- **`WorkoutManager.Domain`**: Contains only the core data models and repository contracts.
- **`WorkoutManager.Data`**: Contains the concrete implementation of data access logic, completely isolated from the business rules.
- **`WorkoutManager.BusinessLogic`**: Contains pure business logic, free from any direct dependency on data access technology.

