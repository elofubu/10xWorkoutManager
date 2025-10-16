using Bogus;
using Moq;
using FluentAssertions;
using WorkoutManager.BusinessLogic.Services.Implementations;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using Xunit;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutManager.BusinessLogic.Tests.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly SessionService _sut;
    private readonly Faker<Session> _sessionFaker;
    private readonly Guid _userId = Guid.NewGuid();

    public SessionServiceTests()
    {
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _sut = new SessionService(_sessionRepositoryMock.Object);
        _sessionFaker = new Faker<Session>()
            .RuleFor(x => x.Id, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.UserId, _userId)
            .RuleFor(x => x.PlanId, f => f.Random.Int(1, 100))
            .RuleFor(x => x.StartTime, f => f.Date.Past());
    }

    [Fact]
    public async Task StartSessionAsync_Should_Throw_BusinessRuleViolationException_When_Active_Session_Exists()
    {
        // Arrange
        _sessionRepositoryMock.Setup(x => x.HasActiveSessionAsync(_userId)).ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.StartSessionAsync(1, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task GetSessionHistoryAsync_Should_Return_Paginated_List()
    {
        // Arrange
        var sessions = _sessionFaker.Generate(10);
        _sessionRepositoryMock.Setup(x => x.GetSessionHistoryAsync(_userId, 1, 10)).ReturnsAsync(sessions);

        // Act
        var result = await _sut.GetSessionHistoryAsync(_userId, 1, 10);

        // Assert
        result.Data.Should().HaveCount(10);
        result.Pagination.TotalCount.Should().Be(10);
    }
    
    [Fact]
    public async Task FinishSessionAsync_Should_Throw_BusinessRuleViolationException_When_Session_Is_Already_Finished()
    {
        // Arrange
        var session = _sessionFaker.Generate();
        session.EndTime = DateTime.UtcNow;
        _sessionRepositoryMock.Setup(x => x.GetSessionByIdAsync((int)session.Id, _userId)).ReturnsAsync(session);

        // Act
        Func<Task> act = () => _sut.FinishSessionAsync((int)session.Id, "notes", _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task GetActiveSessionAsync_Should_Return_Null_When_No_Active_Session()
    {
        // Arrange
        _sessionRepositoryMock.Setup(x => x.GetActiveSessionAsync(_userId)).ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.GetActiveSessionAsync(_userId);

        // Assert
        result.Should().BeNull();
    }
}
