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
using System.Collections.Generic;

namespace WorkoutManager.BusinessLogic.Tests.Services;

public class SessionExerciseServiceTests
{
    private readonly Mock<ISessionExerciseRepository> _sessionExerciseRepositoryMock;
    private readonly SessionExerciseService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public SessionExerciseServiceTests()
    {
        _sessionExerciseRepositoryMock = new Mock<ISessionExerciseRepository>();
        _sut = new SessionExerciseService(_sessionExerciseRepositoryMock.Object);
    }

    [Fact]
    public async Task UpdateSessionExerciseAsync_Should_Throw_NotFoundException_When_Session_Not_Found()
    {
        // Arrange
        var command = new UpdateSessionExerciseCommand();
        _sessionExerciseRepositoryMock.Setup(x => x.GetSessionByIdAndUserIdAsync(1, _userId)).ReturnsAsync((Session?)null);

        // Act
        Func<Task> act = () => _sut.UpdateSessionExerciseAsync(1, 1, command, _userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateSessionExerciseAsync_Should_Update_Exercise_And_Sets()
    {
        // Arrange
        var command = new UpdateSessionExerciseCommand { Sets = new List<UpdateExerciseSetDto> { new UpdateExerciseSetDto() } };
        _sessionExerciseRepositoryMock.Setup(x => x.GetSessionByIdAndUserIdAsync(1, _userId)).ReturnsAsync(new Session());
        _sessionExerciseRepositoryMock.Setup(x => x.GetSessionExerciseByIdAndSessionIdAsync(1, 1)).ReturnsAsync(new SessionExercise());
        _sessionExerciseRepositoryMock.Setup(x => x.AddSetsToSessionExerciseAsync(1, It.IsAny<IEnumerable<ExerciseSet>>())).ReturnsAsync(new List<ExerciseSet> { new ExerciseSet() });


        // Act
        var result = await _sut.UpdateSessionExerciseAsync(1, 1, command, _userId);

        // Assert
        _sessionExerciseRepositoryMock.Verify(x => x.UpdateSessionExerciseAsync(It.IsAny<SessionExercise>()), Times.Once);
        _sessionExerciseRepositoryMock.Verify(x => x.DeleteSetsForSessionExerciseAsync(1), Times.Once);
        _sessionExerciseRepositoryMock.Verify(x => x.AddSetsToSessionExerciseAsync(1, It.IsAny<IEnumerable<ExerciseSet>>()), Times.Once);
        result.Sets.Should().HaveCount(1);
    }

    [Fact]
    public async Task MarkAsSkippedAsync_Should_Throw_NotFoundException_When_Exercise_Not_Found()
    {
        // Arrange
        _sessionExerciseRepositoryMock.Setup(x => x.GetSessionExerciseWithSessionAsync(1, _userId)).ReturnsAsync((SessionExercise?)null);

        // Act
        Func<Task> act = () => _sut.MarkAsSkippedAsync(1, _userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
