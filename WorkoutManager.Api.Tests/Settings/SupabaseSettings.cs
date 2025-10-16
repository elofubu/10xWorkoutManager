namespace WorkoutManager.Api.Tests.Settings
{
    public class SupabaseSettings
    {
        public const string SectionName = "Supabase";

        public string Url { get; set; }
        public string AnonKey { get; set; }
        public Guid TestUserId { get; set; }
    }
}
