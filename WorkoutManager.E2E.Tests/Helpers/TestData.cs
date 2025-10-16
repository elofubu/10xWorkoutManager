namespace WorkoutManager.E2E.Tests.Helpers;

public static class TestData
{
    public static class Users
    {
        public static class User1
        {
            public const string Email = "user1@test.local";
            public const string Password = "Test123!@#";
        }

        public static class User2
        {
            public const string Email = "user2@test.local";
            public const string Password = "Test123!@#";
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
