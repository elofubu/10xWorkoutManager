namespace WorkoutManager.Api.Tests.Settings
{
    internal class JwtBearerSettings
    {
        public const string SectionName = nameof(JwtBearerSettings);

        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
