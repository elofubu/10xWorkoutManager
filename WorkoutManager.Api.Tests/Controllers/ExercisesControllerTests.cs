using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Supabase;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Api.Tests.Controllers;

public class ExercisesControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;
    
    public ExercisesControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
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
        AuthenticateAs(TestUsers.UserAId);

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

        var userAExercises = TestDataGenerator.ExerciseFaker(TestUsers.UserAId, muscleGroupIds).Generate(3);
        await _supabaseClient.From<Exercise>().Insert(userAExercises);

        var userBExercises = TestDataGenerator.ExerciseFaker(TestUsers.UserBId, muscleGroupIds).Generate(2);
        await _supabaseClient.From<Exercise>().Insert(userBExercises);
        
        AuthenticateAs(TestUsers.UserAId);

        // Act
        var response = await HttpClient.GetAsync("/api/exercises");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ExerciseDto>>();
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data.Should().OnlyContain(e => userAExercises.Any(uae => uae.Name == e.Name));
    }
}

