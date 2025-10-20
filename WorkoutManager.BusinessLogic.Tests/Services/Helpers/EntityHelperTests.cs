using FluentAssertions;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Helpers;
using WorkoutManager.Data.Models;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Services.Helpers;

public class EntityHelperTests
{
    #region ThrowIfNotFoundAsync Tests

    [Fact]
    public async Task ThrowIfNotFoundAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        var session = new Session { Id = 1, UserId = Guid.NewGuid(), StartTime = DateTime.UtcNow };
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(session);

        // Act
        var result = await EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "Session", 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(session);
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WhenEntityIsNull_ThrowsNotFoundException()
    {
        // Arrange
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "Session", 1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Session with ID 1 not found");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WithoutId_ErrorMessageDoesNotIncludeId()
    {
        // Arrange
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "Session");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Session not found");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_ErrorMessageIncludesEntityName()
    {
        // Arrange
        Func<Task<Exercise?>> fetchFunc = () => Task.FromResult<Exercise?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "Exercise", 42);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Exercise with ID 42 not found");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_ErrorMessageIncludesProvidedId()
    {
        // Arrange
        var trainerId = "trainer-123";
        Func<Task<TrainingDay?>> fetchFunc = () => Task.FromResult<TrainingDay?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "Training Day", trainerId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Training Day with ID trainer-123 not found");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WhenFetchFuncIsNull_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync<Session>(null!, "Session");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("fetchFunc");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WhenEntityNameIsNull_ThrowsArgumentException()
    {
        // Arrange
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, null!, 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("entityName");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WhenEntityNameIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "", 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("entityName");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WhenEntityNameIsWhitespace_ThrowsArgumentException()
    {
        // Arrange
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "   ", 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("entityName");
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_WithComplexEntity_PreservesAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new Session
        {
            Id = 123,
            UserId = userId,
            PlanId = 456,
            Notes = "Test notes",
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow
        };
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(session);

        // Act
        var result = await EntityHelper.ThrowIfNotFoundAsync(fetchFunc, "Session", 123);

        // Assert
        result.Id.Should().Be(123);
        result.UserId.Should().Be(userId);
        result.PlanId.Should().Be(456);
        result.Notes.Should().Be("Test notes");
        result.StartTime.Should().Be(session.StartTime);
        result.EndTime.Should().Be(session.EndTime);
    }

    [Fact]
    public async Task ThrowIfNotFoundAsync_MultipleCallsWithDifferentEntities_WorkIndependently()
    {
        // Arrange
        var session = new Session { Id = 1, UserId = Guid.NewGuid(), StartTime = DateTime.UtcNow };
        var exercise = new Exercise { Id = 1, MuscleGroupId = 1, Name = "Bench Press" };

        Func<Task<Session?>> sessionFunc = () => Task.FromResult<Session?>(session);
        Func<Task<Exercise?>> exerciseFunc = () => Task.FromResult<Exercise?>(exercise);

        // Act
        var resultSession = await EntityHelper.ThrowIfNotFoundAsync(sessionFunc, "Session");
        var resultExercise = await EntityHelper.ThrowIfNotFoundAsync(exerciseFunc, "Exercise");

        // Assert
        resultSession.Should().Be(session);
        resultExercise.Should().Be(exercise);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenEntityExists_ReturnsTrue()
    {
        // Arrange
        var session = new Session { Id = 1, UserId = Guid.NewGuid(), StartTime = DateTime.UtcNow };
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(session);

        // Act
        var result = await EntityHelper.ExistsAsync(fetchFunc);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityIsNull_ReturnsFalse()
    {
        // Arrange
        Func<Task<Session?>> fetchFunc = () => Task.FromResult<Session?>(null);

        // Act
        var result = await EntityHelper.ExistsAsync(fetchFunc);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenFetchFuncIsNull_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = () => EntityHelper.ExistsAsync<Session>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("fetchFunc");
    }

    [Fact]
    public async Task ExistsAsync_DoesNotThrowException_OnNull()
    {
        // Arrange
        Func<Task<Exercise?>> fetchFunc = () => Task.FromResult<Exercise?>(null);

        // Act
        Func<Task> act = () => EntityHelper.ExistsAsync(fetchFunc);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}
