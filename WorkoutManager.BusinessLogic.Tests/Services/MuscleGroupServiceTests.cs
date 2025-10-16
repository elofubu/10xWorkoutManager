using Bogus;
using Moq;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using Xunit;
using FluentAssertions;

namespace WorkoutManager.BusinessLogic.Tests.Services;

public class MuscleGroupServiceTests
{
    private readonly Mock<IMuscleGroupRepository> _muscleGroupRepositoryMock;
    private readonly MuscleGroupService _sut;
    private readonly Faker<MuscleGroup> _muscleGroupFaker;

    public MuscleGroupServiceTests()
    {
        _muscleGroupRepositoryMock = new Mock<IMuscleGroupRepository>();
        _sut = new MuscleGroupService(_muscleGroupRepositoryMock.Object);
        _muscleGroupFaker = new Faker<MuscleGroup>()
            .RuleFor(x => x.Id, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.Name, f => f.Lorem.Word());
    }

    [Fact]
    public async Task GetAllMuscleGroupsAsync_Should_Return_List_Of_MuscleGroups()
    {
        // Arrange
        var muscleGroups = _muscleGroupFaker.Generate(10);
        _muscleGroupRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(muscleGroups);

        // Act
        var result = await _sut.GetAllMuscleGroupsAsync();

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetMuscleGroupByIdAsync_Should_Return_MuscleGroup_When_Exists()
    {
        // Arrange
        var muscleGroup = _muscleGroupFaker.Generate();
        _muscleGroupRepositoryMock.Setup(x => x.GetByIdAsync((int)muscleGroup.Id)).ReturnsAsync(muscleGroup);

        // Act
        var result = await _sut.GetMuscleGroupByIdAsync((int)muscleGroup.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be((int)muscleGroup.Id);
    }

    [Fact]
    public async Task GetMuscleGroupByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        _muscleGroupRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MuscleGroup?)null);

        // Act
        var result = await _sut.GetMuscleGroupByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }
}
