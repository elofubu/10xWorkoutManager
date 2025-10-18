namespace WorkoutManager.Api.Settings
{
    public class SupabaseSettings
    {
        public const string SectionName = nameof(SupabaseSettings);
        public string Url { get; set; }
        public string Secret { get; set; }
    }
}
