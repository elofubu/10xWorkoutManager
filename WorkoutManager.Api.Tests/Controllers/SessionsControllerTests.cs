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
public class SessionsControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;

    public SessionsControllerTests(IntegrationTestWebAppFactory factory, IntegrationTestDatabaseFixture databaseFixture)
        : base(factory, databaseFixture)
    {
        _supabaseClient = factory.Services.GetRequiredService<Client>();
    }

    #region Authentication & Authorization Tests

    [Fact]
    public async Task GetSessions_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSessionById_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/sessions/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActiveSession_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/sessions/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartSession_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Act
        var command = new StartSessionCommand { TrainingDayId = 1 };
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSession_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Act
        var command = new UpdateSessionCommand { Notes = "Test notes" };
        var response = await HttpClient.PutAsJsonAsync("/api/sessions/1", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Helper Methods

    private async Task<CreatedWorkoutPlanDto> CreateTestWorkoutPlanAsync(Guid userId)
    {
        var planCommand = TestDataGenerator.CreateWorkoutPlanCommandFaker().Generate();
        var response = await HttpClient.PostAsJsonAsync("/api/workoutplans", planCommand);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPlan = await response.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();
        return createdPlan!;
    }

    private async Task<(CreatedWorkoutPlanDto Plan, long TrainingDayId)> CreateTestPlanWithTrainingDayAsync()
    {
        var plan = await CreateTestWorkoutPlanAsync(UserId);

        // Get the first training day from the plan
        var getResponse = await HttpClient.GetAsync($"/api/workoutplans/{plan.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var planDetails = await getResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();
        planDetails.Should().NotBeNull();
        planDetails!.TrainingDays.Should().NotBeEmpty();

        var trainingDayId = planDetails.TrainingDays.First().Id;
        return (plan, trainingDayId);
    }

    #endregion

    #region GET /api/sessions Tests

    [Fact]
    public async Task GetSessions_Should_Return_OK_When_Token_Is_Provided()
    {
        // Arrange
        Authenticate();

        // Act
        var response = await HttpClient.GetAsync("/api/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSessions_Should_Handle_Empty_Results_Gracefully()
    {
        // Arrange
        Authenticate();

        // Act
        var response = await HttpClient.GetAsync("/api/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSessions_Should_Support_Pagination()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        // Create 5 sessions (ending each one before starting the next to avoid conflict)
        for (int i = 0; i < 5; i++)
        {
            var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
            var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var session = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();

            // End the session before starting the next one
            var endCommand = new UpdateSessionCommand { EndTime = DateTime.UtcNow };
            var endResponse = await HttpClient.PutAsJsonAsync($"/api/sessions/{session!.Id}", endCommand);
            endResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Act
        var sessionsResponse = await HttpClient.GetAsync("/api/sessions?page=1&pageSize=2");

        // Assert
        sessionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await sessionsResponse.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
        result.Should().NotBeNull();
        result.Data.Should().HaveCountLessThanOrEqualTo(2);
    }

    #endregion

    #region GET /api/sessions/{id} Tests

    [Fact]
    public async Task GetSessionById_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        Authenticate();
        var nonExistentId = 99999L;

        // Act
        var response = await HttpClient.GetAsync($"/api/sessions/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSessionById_Should_Return_Session_Details_When_User_Owns_Session()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", command);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        createdSession.Should().NotBeNull();

        // Act
        var response = await HttpClient.GetAsync($"/api/sessions/{createdSession!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedSession = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
        retrievedSession.Should().NotBeNull();
        retrievedSession.Id.Should().Be(createdSession.Id);
    }

    [Fact]
    public async Task GetSessions_Should_Return_Only_User_Owned_Sessions()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        // Create 2 sessions for the current user
        for (int i = 0; i < 2; i++)
        {
            var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
            var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var session = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();

            // End the session to allow next session to be created
            var endCommand = new UpdateSessionCommand { EndTime = DateTime.UtcNow };
            var endResponse = await HttpClient.PutAsJsonAsync($"/api/sessions/{session!.Id}", endCommand);
            endResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Act
        var listResponse = await HttpClient.GetAsync("/api/sessions?pageSize=100");

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await listResponse.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }

    #endregion

    #region GET /api/sessions/active Tests

    [Fact]
    public async Task GetActiveSession_Should_Return_NoContent_When_No_Active_Session_Exists()
    {
        // Arrange
        Authenticate();

        // Act
        var response = await HttpClient.GetAsync("/api/sessions/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetActiveSession_Should_Return_OK_When_Active_Session_Exists()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", command);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await HttpClient.GetAsync("/api/sessions/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
        result.Should().NotBeNull();
    }

    #endregion

    #region POST /api/sessions Tests

    [Fact]
    public async Task StartSession_Should_Create_New_Session_With_Valid_Command()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task StartSession_Should_Return_CreatedAtAction_With_Location_Header()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task StartSession_Should_Return_NotFound_When_TrainingDay_Does_Not_Exist()
    {
        // Arrange
        Authenticate();
        var command = new StartSessionCommand { TrainingDayId = 99999 };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartSession_Should_Return_NotFound_When_TrainingDay_ID_Is_Zero()
    {
        // Arrange
        Authenticate();
        var command = new StartSessionCommand { TrainingDayId = 0 }; // Invalid ID

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

        // Assert
        // When TrainingDayId is 0, it's treated as not found because 0 is not a valid training day ID
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartSession_Should_Return_Conflict_When_User_Already_Has_Active_Session()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        // Start first session
        var firstCommand = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var firstResponse = await HttpClient.PostAsJsonAsync("/api/sessions", firstCommand);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to start second session while first is still active
        var secondCommand = new StartSessionCommand { TrainingDayId = (int)trainingDayId };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", secondCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region PUT /api/sessions/{id} Tests

    [Fact]
    public async Task UpdateSession_Should_Update_Session_Notes()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var startCommand = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", startCommand);
        var session = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        session.Should().NotBeNull();

        var updateCommand = new UpdateSessionCommand { Notes = "Updated notes" };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/sessions/{session!.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the notes were updated
        var getResponse = await HttpClient.GetAsync($"/api/sessions/{session.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        result.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateSession_Should_Return_NoContent_On_Success()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var startCommand = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", startCommand);
        var session = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        session.Should().NotBeNull();

        var updateCommand = new UpdateSessionCommand { Notes = "Test notes" };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/sessions/{session!.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateSession_Should_Return_NotFound_When_Session_Does_Not_Exist()
    {
        // Arrange
        Authenticate();
        var command = new UpdateSessionCommand { Notes = "Test notes" };

        // Act
        var response = await HttpClient.PutAsJsonAsync("/api/sessions/99999", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSession_Should_Finish_Session_When_EndTime_Is_Provided()
    {
        // Arrange
        Authenticate();

        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var startCommand = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", startCommand);
        var session = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        session.Should().NotBeNull();
        session!.EndTime.Should().BeNull();

        var finishCommand = new UpdateSessionCommand { EndTime = DateTime.UtcNow };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/sessions/{session.Id}", finishCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the session was finished
        var getResponse = await HttpClient.GetAsync($"/api/sessions/{session.Id}");
        var finishedSession = await getResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        finishedSession.Should().NotBeNull();
        finishedSession!.EndTime.Should().NotBeNull();
    }

    #endregion

    #region Session-Plan Relationship Tests (Nested Query Verification)

    //[Fact]
    //public async Task GetSessions_Should_Include_Plan_Information_Via_Nested_Query()
    //{
    //    // Arrange
    //    Authenticate();

    //    // Create a workout plan with a specific name
    //    var planCommand = TestDataGenerator.CreateWorkoutPlanCommandFaker("Push Day Plan").Generate();
    //    var createPlanResponse = await HttpClient.PostAsJsonAsync("/api/workoutplans", planCommand);
    //    createPlanResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    //    var createdPlan = await createPlanResponse.Content.ReadFromJsonAsync<CreatedWorkoutPlanDto>();
    //    createdPlan.Should().NotBeNull();
    //    createdPlan!.Name.Should().Be("Push Day Plan");

    //    // Get the plan details to find a training day
    //    var getPlanResponse = await HttpClient.GetAsync($"/api/workoutplans/{createdPlan.Id}");
    //    getPlanResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var planDetails = await getPlanResponse.Content.ReadFromJsonAsync<WorkoutPlanDetailDto>();
    //    planDetails.Should().NotBeNull();
    //    planDetails!.TrainingDays.Should().NotBeEmpty();

    //    var trainingDayId = planDetails.TrainingDays.First().Id;

    //    // Create a session using that training day (which links to the plan)
    //    var sessionCommand = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
    //    var createSessionResponse = await HttpClient.PostAsJsonAsync("/api/sessions", sessionCommand);
    //    createSessionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    //    // Act - Get all sessions (uses GetSessionHistoryAsync with nested Select("*, plan(*)"))
    //    var getSessionsResponse = await HttpClient.GetAsync("/api/sessions?pageSize=10");

    //    // Assert - Verify sessions include plan data from nested query
    //    getSessionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var result = await getSessionsResponse.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
    //    result.Should().NotBeNull();
    //    result!.Data.Should().NotBeEmpty();

    //    // Find the session we just created
    //    var sessionFromList = result.Data.FirstOrDefault(s => s.PlanId == createdPlan.Id);
    //    sessionFromList.Should().NotBeNull("session should be in the list");

    //    // Verify the nested plan data is populated
    //    sessionFromList!.PlanId.Should().Be(createdPlan.Id, "session should have the correct plan ID");
    //    sessionFromList.PlanName.Should().Be("Push Day Plan", "session summary should include the plan name from nested query");
    //}

    //[Fact]
    //public async Task GetSessions_Should_Return_Sessions_With_Valid_Plan_References()
    //{
    //    // Arrange
    //    Authenticate();

    //    // Create a session using a training day
    //    var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

    //    var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
    //    var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", command);
    //    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    //    var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
    //    createdSession.Should().NotBeNull();

    //    // End the session to add it to history
    //    var endCommand = new UpdateSessionCommand { EndTime = DateTime.UtcNow };
    //    var endResponse = await HttpClient.PutAsJsonAsync($"/api/sessions/{createdSession!.Id}", endCommand);
    //    endResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    //    // Act
    //    var response = await HttpClient.GetAsync("/api/sessions?pageSize=10");

    //    // Assert
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //    var result = await response.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
    //    result.Should().NotBeNull();
    //    result!.Data.Should().NotBeEmpty();

    //    // Verify the session has valid plan information
    //    var retrievedSession = result.Data.FirstOrDefault(s => s.Id == createdSession.Id);
    //    retrievedSession.Should().NotBeNull("created session should be in history");
    //    retrievedSession!.PlanId.Should().Be(plan.Id, "session should reference the correct plan");
    //    retrievedSession.PlanName.Should().NotBeNullOrEmpty("plan name should be populated from nested query");
    //}

    #endregion

    #region Training Day Information Tests

    [Fact]
    public async Task StartSession_Should_Return_TrainingDayName_When_Started_From_TrainingDay()
    {
        // Arrange
        Authenticate();
        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/sessions", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
        result.Should().NotBeNull();
        result!.TrainingDayId.Should().Be(trainingDayId);
        result.TrainingDayName.Should().NotBeNullOrEmpty("training day name should be populated from the training day");
        result.PlanName.Should().NotBeNullOrEmpty("plan name should be populated from the plan");
        result.PlanId.Should().Be(plan.Id);
    }

    [Fact]
    public async Task GetSessions_Should_Return_TrainingDayName_For_Each_Session()
    {
        // Arrange
        Authenticate();
        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        // Create session from training day
        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", command);
        var session = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        session.Should().NotBeNull();

        // End session
        var endCommand = new UpdateSessionCommand { EndTime = DateTime.UtcNow };
        await HttpClient.PutAsJsonAsync($"/api/sessions/{session!.Id}", endCommand);

        // Act
        var response = await HttpClient.GetAsync("/api/sessions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<SessionSummaryDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);

        var sessionSummary = result.Data[0];
        sessionSummary.TrainingDayId.Should().Be(trainingDayId, "session should be linked to the correct training day");
        sessionSummary.TrainingDayName.Should().NotBeNullOrEmpty("training day name should be populated in session history");
        sessionSummary.PlanName.Should().NotBeNullOrEmpty("plan name should be populated in session history");
        sessionSummary.PlanId.Should().Be(plan.Id);
    }

    [Fact]
    public async Task GetSessionById_Should_Return_TrainingDayName()
    {
        // Arrange
        Authenticate();
        var (plan, trainingDayId) = await CreateTestPlanWithTrainingDayAsync();

        var command = new StartSessionCommand { TrainingDayId = (int)trainingDayId };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/sessions", command);
        var createdSession = await createResponse.Content.ReadFromJsonAsync<SessionDetailsDto>();
        createdSession.Should().NotBeNull();

        // Act
        var response = await HttpClient.GetAsync($"/api/sessions/{createdSession!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedSession = await response.Content.ReadFromJsonAsync<SessionDetailsDto>();
        retrievedSession.Should().NotBeNull();
        retrievedSession!.Id.Should().Be(createdSession.Id);
        retrievedSession.TrainingDayId.Should().Be(trainingDayId);
        retrievedSession.TrainingDayName.Should().NotBeNullOrEmpty("training day name should be populated");
        retrievedSession.PlanName.Should().NotBeNullOrEmpty("plan name should be populated");
        retrievedSession.PlanId.Should().Be(plan.Id);
    }

    #endregion
}
