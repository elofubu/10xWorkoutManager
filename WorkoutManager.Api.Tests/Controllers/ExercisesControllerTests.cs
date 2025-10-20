using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Supabase;
using System.Net;
using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Api.Tests.Controllers;

[Collection("Integration Tests")]
public class ExercisesControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;

    public ExercisesControllerTests(IntegrationTestWebAppFactory factory, IntegrationTestDatabaseFixture databaseFixture) : base(factory, databaseFixture)
    {
        _supabaseClient = factory.Services.GetRequiredService<Client>();
    }

    [Fact]
    public async Task Get_Exercises_Should_Return_Unauthorized_When_No_Token_Is_Provided()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/exercises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_Exercises_Should_Return_Ok_When_Token_Is_Provided()
    {
        // Arrange
        Authenticate();
        // Act
        var response = await HttpClient.GetAsync("/api/exercises");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Get_Exercises_Should_Return_Only_User_Owned_Exercises()
    {
        // Arrange
        var muscleGroups = await _supabaseClient.From<MuscleGroup>().Get();
        var muscleGroupIds = muscleGroups.Models.Select(mg => mg.Id).ToList();

        var userExercises = TestDataGenerator.ExerciseFaker(UserId, muscleGroupIds).Generate(3);
        await _supabaseClient.From<Exercise>().Insert(userExercises);

        Authenticate();

        // Act
        var response = await HttpClient.GetAsync("/api/exercises?pageSize=1000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ExerciseDto>>();
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data.Should().OnlyContain(e => userExercises.Any(uae => uae.Name == e.Name));
    }

    #region Query Optimization Tests

    [Fact]
    public async Task Get_Exercises_Should_Include_Shared_Exercises_Via_Single_OR_Query()
    {
        // Arrange
        Authenticate();

        var muscleGroups = await _supabaseClient.From<MuscleGroup>().Get();
        var muscleGroupId = muscleGroups.Models.First().Id;

        // Create shared exercises (user_id IS NULL)
        var sharedExercises = new List<Exercise>
        {
            new Exercise { UserId = null, MuscleGroupId = muscleGroupId, Name = "Bench Press (Shared)" },
            new Exercise { UserId = null, MuscleGroupId = muscleGroupId, Name = "Squat (Shared)" }
        };
        await _supabaseClient.From<Exercise>().Insert(sharedExercises);

        // Create user-specific exercises
        var userExercises = new List<Exercise>
        {
            new Exercise { UserId = UserId, MuscleGroupId = muscleGroupId, Name = "Custom Bench" }
        };
        await _supabaseClient.From<Exercise>().Insert(userExercises);

        // Act - Get exercises (should use single OR query)
        var response = await HttpClient.GetAsync("/api/exercises?pageSize=1000");

        // Assert - Verify both shared and user exercises are returned
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ExerciseDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().HaveCountGreaterThanOrEqualTo(3,
            "should include 2 shared + 1 user exercise via single OR query");

        // Verify both shared exercises are present
        result.Data.Should().Contain(e => e.Name == "Bench Press (Shared)");
        result.Data.Should().Contain(e => e.Name == "Squat (Shared)");
        result.Data.Should().Contain(e => e.Name == "Custom Bench");
    }

    #endregion
}

