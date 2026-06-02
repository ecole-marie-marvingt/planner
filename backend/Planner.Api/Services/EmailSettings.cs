namespace Planner.Api.Services;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 587;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool UseSsl { get; init; } = true;
    public string FromAddress { get; init; } = "mariemarvingt.noreply@gmail.com";
    public string FromName { get; init; } = "École Marie Marvingt";
    public string? ReplyToAddress { get; init; } = null; // Optionnel, si différent de FromAddress
    public bool Enabled { get; init; } = true;
}
