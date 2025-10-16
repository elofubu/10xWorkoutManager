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
}

