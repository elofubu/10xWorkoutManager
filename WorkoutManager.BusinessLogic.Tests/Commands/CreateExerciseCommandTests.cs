using FluentAssertions;
using Xunit;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Tests.Commands;

public class CreateExerciseCommandTests : BaseTest
{
    [Fact]
    public void CreateExerciseCommand_WithValidData_ShouldBeValid()
    {
        // Arrange
        var command = new CreateExerciseCommand
        {
            Name = "Bench Press",
            MuscleGroupId = 1
        };

        // Assert
        command.Name.Should().Be("Bench Press");
        command.MuscleGroupId.Should().Be(1);
    }

    [Fact]
    public void TestDataBuilder_GeneratesValidCommand()
    {
        // Arrange
        var command = TestDataBuilders.CreateExerciseCommandFaker.Generate();

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().NotBeNullOrEmpty();
        command.MuscleGroupId.Should().BeGreaterThan(0);
    }
}

