namespace WorkoutManager.BusinessLogic.Exceptions;

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message = "Unauthorized access to resource")
        : base(message) { }
}

