using Planner.Api.Models;

namespace Planner.Api.Repositories;

public interface ISlotRepository
{
    Task<IEnumerable<Slot>> GetSlotsAsync(
        DateOnly startDate,
        DateOnly endDate,
        string? userEmail = null,
        CancellationToken ct = default);

    Task<Slot?> GetSlotByIdAsync(
        Guid id,
        string? userEmail = null,
        CancellationToken ct = default);

    Task<Slot> UpdateBookedCountAsync(Guid slotId, CancellationToken ct = default);
}
