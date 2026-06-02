using Microsoft.AspNetCore.Mvc;
using Planner.Api.DTOs;
using Planner.Api.Repositories;

namespace Planner.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController(
    IBookingRepository bookingRepo,
    ISlotRepository slotRepo) : ControllerBase
{
    // GET /api/bookings/cancel/{cancellationToken}
    // Appelé depuis le lien contenu dans l'email de confirmation
    [HttpGet("cancel/{cancellationToken:guid}")]
    [ProducesResponseType<CancelBookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> CancelByTokenAsync(
        Guid cancellationToken,
        CancellationToken ct)
    {
        var booking = await bookingRepo.GetBookingByCancellationTokenAsync(cancellationToken, ct);
        if (booking is null)
            return NotFound("Réservation introuvable ou déjà annulée.");

        await bookingRepo.DeleteBookingAsync(booking.BookingId, ct);

        var updatedSlot = await slotRepo.GetSlotByIdAsync(booking.SlotId, booking.Email, ct);
        if (updatedSlot is null)
            return Ok(new CancelBookingResponse(null!)); // slot supprimé entre temps

        return Ok(new CancelBookingResponse(updatedSlot));
    }
}
