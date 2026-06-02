using Planner.Api.Models;

namespace Planner.Api.Services;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(
        Booking booking,
        Slot slot,
        CancellationToken ct = default);
}
