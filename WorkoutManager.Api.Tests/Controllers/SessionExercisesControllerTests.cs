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
public class SessionExercisesControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;

    public SessionExercisesControllerTests(IntegrationTestWebAppFactory factory, IntegrationTestDatabaseFixture databaseFixture)
        : base(factory, databaseFixture)
    {
        _supabaseClient = factory.Services.GetRequiredService<Client>();
    }

    #region Helper Methods

    private async Task<(CreatedWorkoutPlanDto Plan, long TrainingDayId)> CreateTestPlanWithTrainingDayAsync()
    {
        var planCommand = TestDataGenerator.CreateWorkoutPlanCommandFaker().Generate();
        var response = await HttpClient.PostAsJsonAsync("/api/workoutplans", planCommand);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPlan = await response.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();

        // Get the first training day from the plan
        var getResponse = await HttpClient.GetAsync($"/api/workoutplans/{createdPlan!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var planDetails = await getResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();
        planDetails.Should().NotBeNull();
        planDetails!.TrainingDays.Should().NotBeEmpty();

        var trainingDayId = planDetails.TrainingDays.First().Id;
        return (createdPlan, trainingDayId);
    }

    private async Task<SessionDetailsDto> CreateTestSessionAsync(long trainingDayId)
    {
        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
        return session!;
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task UpdateSessionExercise_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Arrange
        var command = new UpdateSessionExerciseCommand
        {
            Notes = "Test",
            Skipped = true,
            Sets = new()
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync("/api/sessions/1/exercises/1", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT /api/sessions/{sessionId}/exercises/{sessionExerciseId} Tests

    [Fact]
    public async Task UpdateSessionExercise_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        Authenticate();

        var command = new UpdateSessionExerciseCommand
        {
            Notes = "Test",
            Skipped = true,
            Sets = new()
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync("/api/sessions/99999/exercises/1", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSessionExercise_Should_Return_NotFound_When_Exercise_Does_Not_Exist()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();
        var session = await CreateTestSessionAsync(trainingDayId);

        var command = new UpdateSessionExerciseCommand
        {
            Notes = "Test",
            Skipped = true,
            Sets = new()
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/sessions/{session.Id}/exercises/99999", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSessionExercise_Should_Return_BadRequest_When_Validation_Fails()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();
        var session = await CreateTestSessionAsync(trainingDayId);

        // Invalid command: Skipped = false but no sets provided
        var invalidCommand = new UpdateSessionExerciseCommand
        {
            Notes = "Test",
            Skipped = false,
            Sets = new() // Empty sets when not skipped should fail validation
        };

        // Get a session exercise if available
        var getSessionResponse = await HttpClient.GetAsync($"/api/sessions/{session.Id}");
        var sessionDetails = await getSessionResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();

        if (sessionDetails?.Exercises?.Count > 0)
        {
            var sessionExerciseId = sessionDetails.Exercises.First().Id;

            // Act
            var response = await HttpClient.PutAsJsonAsync($"/api/sessions/{session.Id}/exercises/{sessionExerciseId}", invalidCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task UpdateSessionExercise_Should_Accept_Skipped_Exercise_Without_Sets()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();
        var session = await CreateTestSessionAsync(trainingDayId);

        // Valid command: Skipped = true, no sets required
        var skippedCommand = new UpdateSessionExerciseCommand
        {
            Notes = "Skipped due to injury",
            Skipped = true,
            Sets = new()
        };

        // Get a session exercise if available
        var getSessionResponse = await HttpClient.GetAsync($"/api/sessions/{session.Id}");
        var sessionDetails = await getSessionResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();

        if (sessionDetails?.Exercises?.Count > 0)
        {
            var sessionExerciseId = sessionDetails.Exercises.First().Id;

            // Act
            var response = await HttpClient.PutAsJsonAsync($"/api/sessions/{session.Id}/exercises/{sessionExerciseId}", skippedCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<SessionExerciseDetailsDto>();
            result.Should().NotBeNull();
            result!.Skipped.Should().BeTrue();
            result.Notes.Should().Be("Skipped due to injury");
        }
    }

    #endregion
}
