using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Api.Tests.Controllers;

public class WorkoutPlansControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;
    public WorkoutPlansControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _supabaseClient = factory.Services.GetRequiredService<Client>();
    }
    
    [Fact]
    public async Task Create_WorkoutPlan_Should_Return_Created_When_Payload_Is_Valid()
    {
        // Arrange
        AuthenticateAs(TestUsers.UserAId);
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
        AuthenticateAs(TestUsers.UserAId);
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
        AuthenticateAs(TestUsers.UserAId);

        // Act
        var response = await HttpClient.GetAsync("/api/workoutplans/999");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Get_WorkoutPlan_Should_Return_Plan_When_It_Exists()
    {
        // Arrange
        AuthenticateAs(TestUsers.UserAId);
        var plan = TestDataGenerator.WorkoutPlanFaker(TestUsers.UserAId).Generate();
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
        AuthenticateAs(TestUsers.UserAId);
        var plans = TestDataGenerator.WorkoutPlanFaker(TestUsers.UserAId).Generate(5);
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
        AuthenticateAs(TestUsers.UserAId);
        var plan = TestDataGenerator.WorkoutPlanFaker(TestUsers.UserAId).Generate();
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
        AuthenticateAs(TestUsers.UserAId);
        var plan = TestDataGenerator.WorkoutPlanFaker(TestUsers.UserAId).Generate();
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
}
