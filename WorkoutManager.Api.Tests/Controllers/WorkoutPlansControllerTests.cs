using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Supabase;
using System.Net;
using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Api.Tests.Controllers;

[Collection("Integration Tests")]
public class WorkoutPlansControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;
    public WorkoutPlansControllerTests(IntegrationTestWebAppFactory factory, IntegrationTestDatabaseFixture databaseFixture) : base(factory, databaseFixture)
    {
        _supabaseClient = factory.Services.GetRequiredService<Client>();
    }

    [Fact]
    public async Task Create_WorkoutPlan_Should_Return_Created_When_Payload_Is_Valid()
    {
        // Arrange
        Authenticate();
        var command = TestDataGenerator.CreateWorkoutPlanCommandFaker().Generate();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/workoutplans", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPlan = await response.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();
        createdPlan.Should().NotBeNull();
        createdPlan.Name.Should().Be(command.Name);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"http://localhost/api/WorkoutPlans/{createdPlan.Id}");
    }

    [Fact]
    public async Task Create_WorkoutPlan_Should_Return_BadRequest_When_Payload_Is_Invalid()
    {
        // Arrange
        Authenticate();
        var command = new CreateWorkoutPlanCommand("", new List<CreateTrainingDayCommand>());

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/workoutplans", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_WorkoutPlan_Should_Return_NotFound_When_Plan_Does_Not_Exist()
    {
        // Arrange
        Authenticate();

        // Act
        var response = await HttpClient.GetAsync("/api/workoutplans/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_WorkoutPlan_Should_Return_Plan_When_It_Exists()
    {
        // Arrange
        Authenticate();
        var plan = TestDataGenerator.WorkoutPlanFaker(UserId).Generate();
        var inserted = await _supabaseClient.From<WorkoutPlan>().Insert(plan);
        var planId = inserted.Models.First().Id;

        // Act
        var response = await HttpClient.GetAsync($"/api/workoutplans/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var workoutPlan = await response.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();
        workoutPlan.Should().NotBeNull();
        workoutPlan.Id.Should().Be(planId);
        workoutPlan.Name.Should().Be(plan.Name);
    }

    [Fact]
    public async Task Get_WorkoutPlans_Should_Return_Paginated_List_Of_Plans()
    {
        // Arrange
        Authenticate();
        var plans = TestDataGenerator.WorkoutPlanFaker(UserId).Generate(5);
        await _supabaseClient.From<WorkoutPlan>().Insert(plans);

        // Act
        var response = await HttpClient.GetAsync("/api/workoutplans?page=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<WorkoutPlanDto>>();
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Pagination.TotalCount.Should().Be(5);
        result.Pagination.Page.Should().Be(1);
        result.Pagination.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Delete_WorkoutPlan_Should_Return_NoContent_When_Successful()
    {
        // Arrange
        Authenticate();
        var plan = TestDataGenerator.WorkoutPlanFaker(UserId).Generate();
        var inserted = await _supabaseClient.From<WorkoutPlan>().Insert(plan);
        var planId = inserted.Models.First().Id;

        // Act
        var response = await HttpClient.DeleteAsync($"/api/workoutplans/{planId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await HttpClient.GetAsync($"/api/workoutplans/{planId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WorkoutPlan_Should_Return_NoContent_When_Successful()
    {
        // Arrange
        Authenticate();
        var plan = TestDataGenerator.WorkoutPlanFaker(UserId).Generate();
        var inserted = await _supabaseClient.From<WorkoutPlan>().Insert(plan);
        var planId = inserted.Models.First().Id;

        var updatePayload = new UpdateWorkoutPlanPayload("Updated Name", new List<UpdateTrainingDayOrderCommand>());

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/workoutplans/{planId}", updatePayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedPlanResponse = await _supabaseClient.From<WorkoutPlan>().Filter("id", Supabase.Postgrest.Constants.Operator.Equals, planId).Single();
        updatedPlanResponse.Should().NotBeNull();
        updatedPlanResponse.Name.Should().Be("Updated Name");
    }

    #region Nested Query Optimization Tests (GetTrainingDaysWithExercisesAsync)

    [Fact]
    public async Task Get_WorkoutPlan_Should_Include_Complete_Training_Day_Hierarchy_Via_Nested_Query()
    {
        // Arrange
        Authenticate();

        // Create a plan with multiple training days and exercises
        var createPlanCommand = new CreateWorkoutPlanCommand(
            "Full Body Workout",
            new List<CreateTrainingDayCommand>
            {
                new CreateTrainingDayCommand("Day A - Chest", 1),
                new CreateTrainingDayCommand("Day B - Legs", 2)
            }
        );

        var createPlanResponse = await HttpClient.PostAsJsonAsync("/api/workoutplans", createPlanCommand);
        createPlanResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPlan = await createPlanResponse.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();
        createdPlan.Should().NotBeNull();

        // Get the plan details to retrieve training days
        var getPlanResponse = await HttpClient.GetAsync($"/api/workoutplans/{createdPlan!.Id}");
        getPlanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var planDetails = await getPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();

        // Act & Assert - Verify nested query returned complete hierarchy
        planDetails.Should().NotBeNull();
        planDetails!.TrainingDays.Should().HaveCount(2, "plan should have 2 training days");

        // Verify training days are in correct order
        planDetails.TrainingDays.First().Name.Should().Be("Day A - Chest");
        planDetails.TrainingDays.Last().Name.Should().Be("Day B - Legs");

        // Verify nested query structure is populated (even if no exercises yet)
        foreach (var day in planDetails.TrainingDays)
        {
            day.Should().NotBeNull("each training day should be populated from nested query");
        }
    }

    [Fact]
    public async Task Get_WorkoutPlan_Should_Return_Training_Days_With_Exercises_From_Single_Query()
    {
        // Arrange
        Authenticate();

        // Create plan with training days
        var createPlanCommand = new CreateWorkoutPlanCommand(
            "Push Day Plan",
            new List<CreateTrainingDayCommand>
            {
                new CreateTrainingDayCommand("Upper Push", 1),
                new CreateTrainingDayCommand("Lower Push", 2)
            }
        );
        //var createPlanCommand = new CreateWorkoutPlanCommand(
        //    "Push Day Plan",
        //    new List<CreateTrainingDayCommand>
        //    {
        //        new CreateTrainingDayCommand("Upper Push", 1),
        //        new CreateTrainingDayCommand("Lower Push", 2)
        //    }
        //);

        var createPlanResponse = await HttpClient.PostAsJsonAsync("/api/workoutplans", createPlanCommand);
        var createdPlan = await createPlanResponse.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();

        // Get training days to add exercises to them
        var getPlanResponse = await HttpClient.GetAsync($"/api/workoutplans/{createdPlan!.Id}");
        var planDetails = await getPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();

        // Get the actual training day from the database to ensure we have the correct ID
        var trainingDaysResponse = await _supabaseClient
            .From<TrainingDay>()
            .Filter("plan_id", Supabase.Postgrest.Constants.Operator.Equals, createdPlan.Id)
            .Get();

        var firstTrainingDay = trainingDaysResponse.Models.First();

        // Add exercises to the training day
        var muscleGroups = await _supabaseClient.From<MuscleGroup>().Get();
        var chestMuscleGroup = muscleGroups.Models.FirstOrDefault() ??
            (await _supabaseClient.From<MuscleGroup>().Insert(new MuscleGroup { Name = "Chest" })).Models.First();

        var exercises = new List<Exercise>
        {
            new Exercise { UserId = null, MuscleGroupId = chestMuscleGroup.Id, Name = "Bench Press" },
            new Exercise { UserId = null, MuscleGroupId = chestMuscleGroup.Id, Name = "Incline Press" }
        };

        var insertedExercises = (await _supabaseClient.From<Exercise>().Insert(exercises)).Models;

        // Add exercises to training day via direct database insertion
        var planDayExercises = insertedExercises.Select((ex, idx) => new PlanDayExercise
        {
            TrainingDayId = firstTrainingDay.Id,
            ExerciseId = ex.Id,
            Order = (short)(idx + 1)
        }).ToList();

        await _supabaseClient.From<PlanDayExercise>().Upsert(planDayExercises);

        // Act - Get the plan again to verify exercises are in the nested query
        var updatedPlanResponse = await HttpClient.GetAsync($"/api/workoutplans/{createdPlan.Id}");
        updatedPlanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPlanDetails = await updatedPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();

        // Assert - Verify nested query includes exercises
        updatedPlanDetails.Should().NotBeNull();
        updatedPlanDetails!.TrainingDays.Should().NotBeEmpty();

        var dayWithExercises = updatedPlanDetails.TrainingDays.First();
        dayWithExercises.Exercises.Should().HaveCount(2,
            "training day should have 2 exercises from nested query projection");

        // Verify exercises are properly ordered
        dayWithExercises.Exercises.ElementAt(0).Order.Should().Be(1);
        dayWithExercises.Exercises.ElementAt(1).Order.Should().Be(2);

        // Verify exercise details are populated from nested query
        dayWithExercises.Exercises.Should().AllSatisfy(pde =>
        {
            pde.ExerciseId.Should().BeGreaterThan(0, "exercise ID should be populated from nested query");
        });
    }

    //[Fact]
    //public async Task Get_WorkoutPlan_Should_Handle_Multiple_Training_Days_With_Different_Exercise_Counts()
    //{
    //    // Arrange
    //    Authenticate();

    //    var createPlanCommand = new CreateWorkoutPlanCommand(
    //        "Complex Workout",
    //        new List<CreateTrainingDayCommand>
    //        {
    //            new CreateTrainingDayCommand("Day 1", 1),
    //            new CreateTrainingDayCommand("Day 2", 2),
    //            new CreateTrainingDayCommand("Day 3", 3)
    //        }
    //    );

    //    var createResponse = await HttpClient.PostAsJsonAsync("/api/workoutplans", createPlanCommand);
    //    var plan = await createResponse.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();

    //    // Add different numbers of exercises to each training day
    //    var muscleGroups = await _supabaseClient.From<MuscleGroup>().Get();
    //    var muscleGroup = muscleGroups.Models.FirstOrDefault() ??
    //        (await _supabaseClient.From<MuscleGroup>().Insert(new MuscleGroup { Name = "General" })).Models.First();

    //    // Create exercises
    //    var exerciseList = new List<Exercise>();
    //    for (int i = 0; i < 5; i++)
    //    {
    //        exerciseList.Add(new Exercise
    //        {
    //            UserId = null,
    //            MuscleGroupId = muscleGroup.Id,
    //            Name = $"Exercise {i + 1}"
    //        });
    //    }
    //    var insertedExercises = (await _supabaseClient.From<Exercise>().Insert(exerciseList)).Models;

    //    // Add 1 exercise to Day 1, 2 to Day 2, 2 to Day 3
    //    var pdeList = new List<PlanDayExercise>();
    //    var trainingDays = plan!.TrainingDays.ToList();

    //    // Day 1: 1 exercise
    //    pdeList.Add(new PlanDayExercise { TrainingDayId = trainingDays[0].Id, ExerciseId = insertedExercises[0].Id, Order = 1 });

    //    // Day 2: 2 exercises
    //    pdeList.Add(new PlanDayExercise { TrainingDayId = trainingDays[1].Id, ExerciseId = insertedExercises[1].Id, Order = 1 });
    //    pdeList.Add(new PlanDayExercise { TrainingDayId = trainingDays[1].Id, ExerciseId = insertedExercises[2].Id, Order = 2 });

    //    // Day 3: 2 exercises
    //    pdeList.Add(new PlanDayExercise { TrainingDayId = trainingDays[2].Id, ExerciseId = insertedExercises[3].Id, Order = 1 });
    //    pdeList.Add(new PlanDayExercise { TrainingDayId = trainingDays[2].Id, ExerciseId = insertedExercises[4].Id, Order = 2 });

    //    await _supabaseClient.From<PlanDayExercise>().Insert(pdeList);

    //    // Act
    //    var finalPlanResponse = await HttpClient.GetAsync($"/api/workoutplans/{plan.Id}");
    //    var finalPlanDetails = await finalPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();

    //    // Assert - Verify nested query correctly populated all hierarchies
    //    finalPlanDetails.Should().NotBeNull();
    //    finalPlanDetails!.TrainingDays.Should().HaveCount(3);

    //    var days = finalPlanDetails.TrainingDays.ToList();
    //    days[0].Exercises.Should().HaveCount(1, "Day 1 should have 1 exercise");
    //    days[1].Exercises.Should().HaveCount(2, "Day 2 should have 2 exercises");
    //    days[2].Exercises.Should().HaveCount(2, "Day 3 should have 2 exercises");

    //    // Verify all exercises are populated
    //    foreach (var day in days)
    //    {
    //        foreach (var exercise in day.Exercises)
    //        {
    //            exercise.ExerciseId.Should().BeGreaterThan(0, "all nested exercises should be populated");
    //        }
    //    }
    //}

    [Fact]
    public async Task Get_WorkoutPlan_Should_Return_Empty_Training_Days_When_No_Exercises()
    {
        // Arrange
        Authenticate();

        var createPlanCommand = new CreateWorkoutPlanCommand(
            "Empty Workout",
            new List<CreateTrainingDayCommand>
            {
                new CreateTrainingDayCommand("Empty Day", 1)
            }
        );

        var createResponse = await HttpClient.PostAsJsonAsync("/api/workoutplans", createPlanCommand);
        var plan = await createResponse.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();

        // Act
        var getPlanResponse = await HttpClient.GetAsync($"/api/workoutplans/{plan!.Id}");
        var planDetails = await getPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();

        // Assert - Nested query should still work with empty collections
        planDetails.Should().NotBeNull();
        planDetails!.TrainingDays.Should().HaveCount(1);
        planDetails.TrainingDays.First().Exercises.Should().BeEmpty();
    }

    #endregion
}
