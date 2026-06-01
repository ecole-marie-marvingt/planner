namespace Planner.Api.Models;

public sealed class Slot
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Capacity { get; init; }
    public int BookedCount { get; init; }
    public SlotStatus Status { get; init; }

    // Champs calculés côté API selon le contexte utilisateur
    public bool IsBookedByMe { get; init; }
    public Guid? BookingId { get; init; }
}
