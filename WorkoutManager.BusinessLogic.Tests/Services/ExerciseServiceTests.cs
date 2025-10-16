using Bogus;
using Moq;
using FluentAssertions;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using Xunit;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;

namespace WorkoutManager.BusinessLogic.Tests.Services;

public class ExerciseServiceTests
{
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly ExerciseService _sut;
    private readonly Faker<Exercise> _exerciseFaker;
    private readonly Guid _userId = Guid.NewGuid();

    public ExerciseServiceTests()
    {
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _sut = new ExerciseService(_exerciseRepositoryMock.Object);
        _exerciseFaker = new Faker<Exercise>()
            .RuleFor(x => x.Id, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.Name, f => f.Lorem.Word())
            .RuleFor(x => x.UserId, _userId)
            .RuleFor(x => x.MuscleGroupId, f => f.Random.Int(1, 10));
    }

    [Fact]
    public async Task GetExercisesAsync_Should_Return_Filtered_And_Paginated_Exercises()
    {
        // Arrange
        var exercises = _exerciseFaker.Generate(20);
        _exerciseRepositoryMock.Setup(x => x.GetExercisesForUserAsync(_userId)).ReturnsAsync(exercises);

        // Act
        var result = await _sut.GetExercisesAsync(_userId, exercises[0].Name, (int)exercises[0].MuscleGroupId, 1, 5);

        // Assert
        result.Data.Should().NotBeEmpty();
        result.Pagination.TotalCount.Should().Be(exercises.Count(e => e.Name.Contains(exercises[0].Name) && e.MuscleGroupId == exercises[0].MuscleGroupId));
    }

    [Fact]
    public async Task GetExerciseByIdAsync_Should_Return_Exercise_When_Found()
    {
        // Arrange
        var exercise = _exerciseFaker.Generate();
        _exerciseRepositoryMock.Setup(x => x.GetExerciseByIdAsync((int)exercise.Id)).ReturnsAsync(exercise);

        // Act
        var result = await _sut.GetExerciseByIdAsync((int)exercise.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be((int)exercise.Id);
    }

    [Fact]
    public async Task CreateExerciseAsync_Should_Create_Exercise_When_Name_Is_Unique()
    {
        // Arrange
        var createDto = new CreateExerciseDto { Name = "New Exercise", MuscleGroupId = 1 };
        _exerciseRepositoryMock.Setup(x => x.GetExerciseByNameForUserAsync(createDto.Name, _userId)).ReturnsAsync((Exercise?)null);
        _exerciseRepositoryMock.Setup(x => x.CreateExerciseAsync(It.IsAny<Exercise>())).ReturnsAsync((Exercise e) => {
            e.Id = 1;
            return e;
        });

        // Act
        var result = await _sut.CreateExerciseAsync(createDto, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task CreateExerciseAsync_Should_Throw_Exception_When_Name_Exists()
    {
        // Arrange
        var createDto = new CreateExerciseDto { Name = "Existing Exercise", MuscleGroupId = 1 };
        _exerciseRepositoryMock.Setup(x => x.GetExerciseByNameForUserAsync(createDto.Name, _userId)).ReturnsAsync(new Exercise());

        // Act
        Func<Task> act = () => _sut.CreateExerciseAsync(createDto, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task GetLastPerformanceAsync_Should_Return_Performance_Data()
    {
        // Arrange
        var performanceDto = new PreviousExercisePerformanceDto();
        _exerciseRepositoryMock.Setup(x => x.GetLastPerformanceAsync(1, _userId)).ReturnsAsync(performanceDto);

        // Act
        var result = await _sut.GetLastPerformanceAsync(1, _userId);

        // Assert
        result.Should().Be(performanceDto);
    }
}
