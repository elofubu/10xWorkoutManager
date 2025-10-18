namespace WorkoutManager.Api.Settings
{
    public class JwtBearerSettings
    {
        public static string SectionName = nameof(JwtBearerSettings);
        public string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public string IssuerSigningKey { get; set; }
        public string ValidIssuer { get; set; }
    }
}
