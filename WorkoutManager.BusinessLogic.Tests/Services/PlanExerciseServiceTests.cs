using Bogus;
using Moq;
using FluentAssertions;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using Xunit;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Exceptions;
using System;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Tests.Services;

public class PlanExerciseServiceTests
{
    private readonly Mock<IPlanExerciseRepository> _planExerciseRepositoryMock;
    private readonly Mock<IWorkoutPlanService> _workoutPlanServiceMock;
    private readonly PlanExerciseService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public PlanExerciseServiceTests()
    {
        _planExerciseRepositoryMock = new Mock<IPlanExerciseRepository>();
        _workoutPlanServiceMock = new Mock<IWorkoutPlanService>();
        _sut = new PlanExerciseService(_planExerciseRepositoryMock.Object, _workoutPlanServiceMock.Object);
    }

    [Fact]
    public async Task AddExerciseToDayAsync_Should_Throw_BusinessRuleViolationException_When_Plan_Is_Locked()
    {
        // Arrange
        var command = new AddExerciseToTrainingDayCommand { ExerciseId = 1, Order = 1 };
        _planExerciseRepositoryMock.Setup(x => x.GetPlanByIdAndUserIdAsync(1, _userId)).ReturnsAsync(new WorkoutPlan());
        _workoutPlanServiceMock.Setup(x => x.IsPlanLockedAsync(1, _userId)).ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.AddExerciseToDayAsync(1, 1, command, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task RemoveExerciseFromDayAsync_Should_Throw_NotFoundException_When_Training_Day_Not_Found()
    {
        // Arrange
        _planExerciseRepositoryMock.Setup(x => x.GetPlanByIdAndUserIdAsync(1, _userId)).ReturnsAsync(new WorkoutPlan());
        _workoutPlanServiceMock.Setup(x => x.IsPlanLockedAsync(1, _userId)).ReturnsAsync(false);
        _planExerciseRepositoryMock.Setup(x => x.GetTrainingDayByIdAndPlanIdAsync(1, 1)).ReturnsAsync((TrainingDay?)null);

        // Act
        Func<Task> act = () => _sut.RemoveExerciseFromDayAsync(1, 1, 1, _userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ReorderExercisesAsync_Should_Call_Repository_When_Valid()
    {
        // Arrange
        var commands = new System.Collections.Generic.List<ReorderExerciseCommand>();
        _planExerciseRepositoryMock.Setup(x => x.GetPlanByIdAndUserIdAsync(1, _userId)).ReturnsAsync(new WorkoutPlan());
        _workoutPlanServiceMock.Setup(x => x.IsPlanLockedAsync(1, _userId)).ReturnsAsync(false);
        _planExerciseRepositoryMock.Setup(x => x.GetTrainingDayByIdAndPlanIdAsync(1, 1)).ReturnsAsync(new TrainingDay());
        
        // Act
        await _sut.ReorderExercisesAsync(1, 1, commands, _userId);

        // Assert
        _planExerciseRepositoryMock.Verify(x => x.ReorderExercisesAsync(1, commands), Times.Once);
    }
}
