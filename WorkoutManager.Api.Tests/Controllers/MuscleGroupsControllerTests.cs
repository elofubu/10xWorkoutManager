using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Supabase;
using System.Net;
using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Api.Tests.Controllers;

[Collection("Integration Tests")]
public class MuscleGroupsControllerTests : BaseIntegrationTest
{
    private readonly Client _supabaseClient;

    public MuscleGroupsControllerTests(IntegrationTestWebAppFactory factory, IntegrationTestDatabaseFixture databaseFixture)
        : base(factory, databaseFixture)
    {
        _supabaseClient = factory.Services.GetRequiredService<Client>();
    }

    [Fact]
    public async Task GetMuscleGroups_Should_Return_OK_With_All_Muscle_Groups()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/musclegroups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<MuscleGroupDto>>();
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMuscleGroupById_Should_Return_Muscle_Group_When_It_Exists()
    {
        // Arrange
        var muscleGroups = await _supabaseClient.From<MuscleGroup>().Get();
        var muscleGroupIds = muscleGroups.Models.Select(mg => mg.Id).ToList();
        muscleGroupIds.Should().NotBeEmpty();
        var testMuscleGroupId = muscleGroupIds.First();

        // Act
        var response = await HttpClient.GetAsync($"/api/musclegroups/{testMuscleGroupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MuscleGroupDto>();
        result.Should().NotBeNull();
        result.Id.Should().Be(testMuscleGroupId);
    }

    [Fact]
    public async Task GetMuscleGroupById_Should_Return_NotFound_When_Muscle_Group_Does_Not_Exist()
    {
        // Arrange
        var nonExistentId = 99999L;

        // Act
        var response = await HttpClient.GetAsync($"/api/musclegroups/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMuscleGroups_Should_Be_Accessible_Without_Authentication()
    {
        // Act - Note: No Authenticate() call
        var response = await HttpClient.GetAsync("/api/musclegroups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
