namespace WorkoutManager.E2E.Tests.Helpers;

public static class TestData
{
    public static class Users
    {
        public static class User1
        {
            public const string Email = "testa@test.com";
            public const string Password = "1234567890";
        }

        public static class User2
        {
            public const string Email = "testb@test.com";
            public const string Password = "0987654321";
        }

        public static class InvalidUser
        {
            public const string Email = "invalid@test.local";
            public const string Password = "WrongPassword123";
        }
    }

    public static class Routes
    {
        public const string Home = "/";
        public const string Login = "/authentication/login";
        public const string Register = "/authentication/register";
        public const string ResetPassword = "/authentication/reset-password";
        public const string Plans = "/";
        public const string History = "/history";
        public const string Settings = "/settings";
    }

    public static class TestPlans
    {
        public const string DefaultPlanName = "E2E Test Workout Plan";
        public const string DefaultPlanDescription = "Automated test plan";
        public const string DefaultDayName = "Test Day 1";
    }

    public static class TestExercises
    {
        public static readonly string[] ExerciseNames = new[]
        {
            "Bench Press",
            "Squat",
            "Deadlift",
            "Pull Up"
        };
    }
}
