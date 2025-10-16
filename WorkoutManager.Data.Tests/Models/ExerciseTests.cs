using FluentAssertions;
using Xunit;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Data.Tests.Models;

public class ExerciseTests : BaseTest
{
    [Fact]
    public void Exercise_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var exercise = new Exercise
        {
            Id = 1,
            Name = "Squat",
            MuscleGroupId = 1,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        exercise.Id.Should().Be(1);
        exercise.Name.Should().Be("Squat");
        exercise.MuscleGroupId.Should().Be(1);
        exercise.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Exercise_DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var exercise = new Exercise();

        // Assert
        exercise.Id.Should().Be(0);
        exercise.Name.Should().NotBeNull();
    }
}

