namespace WorkoutManager.Api.Settings;

public class EntraExternalIdSettings
{
    public const string SectionName = "EntraExternalId";

    /// <summary>
    /// The external tenant's v2.0 issuer, e.g. https://{subdomain}.ciamlogin.com/{tenant-id}/v2.0
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>The Api app registration's Application (client) ID.</summary>
    public string Audience { get; set; } = string.Empty;
}
