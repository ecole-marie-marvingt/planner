namespace Planner.Api.Models;

public sealed class Booking
{
    public Guid BookingId { get; init; }
    public Guid SlotId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTimeOffset BookedAt { get; init; }
    public Guid CancellationToken { get; init; }

    /// <summary>URL complète de la page d'annulation (injectée après construction).</summary>
    public string CancellationUrl { get; set; } = string.Empty;
}
