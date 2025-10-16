using Bogus;
using Moq;
using FluentAssertions;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using Xunit;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Tests.Services;

public class WorkoutPlanServiceTests
{
    private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepositoryMock;
    private readonly WorkoutPlanService _sut;
    private readonly Faker<WorkoutPlan> _workoutPlanFaker;
    private readonly Guid _userId = Guid.NewGuid();

    public WorkoutPlanServiceTests()
    {
        _workoutPlanRepositoryMock = new Mock<IWorkoutPlanRepository>();
        _sut = new WorkoutPlanService(_workoutPlanRepositoryMock.Object);
        _workoutPlanFaker = new Faker<WorkoutPlan>()
            .RuleFor(x => x.Id, f => f.Random.Long(1, 1000))
            .RuleFor(x => x.Name, f => f.Lorem.Word())
            .RuleFor(x => x.UserId, _userId)
            .RuleFor(x => x.CreatedAt, f => f.Date.Past());
    }

    [Fact]
    public async Task GetWorkoutPlansAsync_Should_Return_Paginated_List()
    {
        // Arrange
        var plans = _workoutPlanFaker.Generate(10);
        _workoutPlanRepositoryMock.Setup(x => x.GetWorkoutPlansAsync(_userId)).ReturnsAsync(plans);

        // Act
        var result = await _sut.GetWorkoutPlansAsync(_userId, 2, 5);

        // Assert
        result.Data.Should().HaveCount(5);
        result.Pagination.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task GetWorkoutPlanByIdAsync_Should_Throw_NotFoundException_When_Plan_Does_Not_Exist()
    {
        // Arrange
        _workoutPlanRepositoryMock.Setup(x => x.GetWorkoutPlanByIdAsync(It.IsAny<long>(), _userId)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        Func<Task> act = () => _sut.GetWorkoutPlanByIdAsync(1, _userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateWorkoutPlanAsync_Should_Create_And_Return_Dto()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand("New Plan", new List<CreateTrainingDayCommand>());
        var plan = new WorkoutPlan { Id = 1, Name = command.Name };
        _workoutPlanRepositoryMock.Setup(x => x.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>(), command.TrainingDays)).ReturnsAsync(plan);

        // Act
        var result = await _sut.CreateWorkoutPlanAsync(command, _userId);

        // Assert
        result.Id.Should().Be(plan.Id);
        result.Name.Should().Be(plan.Name);
    }

    [Fact]
    public async Task UpdateWorkoutPlanAsync_Should_Throw_NotFoundException_When_Plan_Does_Not_Exist()
    {
        // Arrange
        var payload = new UpdateWorkoutPlanPayload("Updated Plan", new List<UpdateTrainingDayOrderCommand>());
        _workoutPlanRepositoryMock.Setup(x => x.GetWorkoutPlanByIdAsync(It.IsAny<long>(), _userId)).ReturnsAsync((WorkoutPlan?)null);

        // Act
        Func<Task> act = () => _sut.UpdateWorkoutPlanAsync(1, payload, _userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task UpdateWorkoutPlanAsync_Should_Throw_BusinessRuleViolationException_When_Plan_Is_Locked()
    {
        // Arrange
        var plan = _workoutPlanFaker.Generate();
        var payload = new UpdateWorkoutPlanPayload("Updated Plan", new List<UpdateTrainingDayOrderCommand>());
        _workoutPlanRepositoryMock.Setup(x => x.GetWorkoutPlanByIdAsync(plan.Id, _userId)).ReturnsAsync(plan);
        _workoutPlanRepositoryMock.Setup(x => x.IsPlanLockedAsync(plan.Id, _userId)).ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.UpdateWorkoutPlanAsync(plan.Id, payload, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task DeleteWorkoutPlanAsync_Should_Throw_BusinessRuleViolationException_When_Plan_Is_Locked()
    {
        // Arrange
        var plan = _workoutPlanFaker.Generate();
        _workoutPlanRepositoryMock.Setup(x => x.GetWorkoutPlanByIdAsync(plan.Id, _userId)).ReturnsAsync(plan);
        _workoutPlanRepositoryMock.Setup(x => x.IsPlanLockedAsync(plan.Id, _userId)).ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.DeleteWorkoutPlanAsync(plan.Id, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }
}
