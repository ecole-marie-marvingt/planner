using Planner.Api.Models;

namespace Planner.Api.Repositories;

public interface IBookingRepository
{
    Task<Planner.Api.Models.Booking?> GetBookingAsync(
        Guid bookingId,
        CancellationToken ct = default);

    Task<Planner.Api.Models.Booking?> GetBookingByCancellationTokenAsync(
        Guid cancellationToken,
        CancellationToken ct = default);

    Task<Planner.Api.Models.Booking> CreateBookingAsync(
        Guid slotId,
        string userName,
        string email,
        string phoneNumber,
        CancellationToken ct = default);

    Task DeleteBookingAsync(Guid bookingId, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid slotId, string email, CancellationToken ct = default);
}
