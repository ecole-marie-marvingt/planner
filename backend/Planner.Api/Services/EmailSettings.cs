namespace Planner.Api.Services;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 587;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool UseSsl { get; init; } = true;
    public string FromAddress { get; init; } = "noreply@ecole-marie-marvingt.fr";
    public string FromName { get; init; } = "École Marie Marvingt";
    public bool Enabled { get; init; } = true;
}
