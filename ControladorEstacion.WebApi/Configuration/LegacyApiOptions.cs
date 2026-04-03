namespace ControladorEstacion.WebApi.Configuration;

public sealed class LegacyApiOptions
{
    public const string SectionName = "LegacyApi";

    public string BaseUrl { get; set; } = "http://localhost:5000";
}
