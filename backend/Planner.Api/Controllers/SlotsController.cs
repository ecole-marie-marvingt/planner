using Microsoft.AspNetCore.Mvc;
using Planner.Api.DTOs;
using Planner.Api.Models;
using Planner.Api.Repositories;
using Planner.Api.Services;

namespace Planner.Api.Controllers;

[ApiController]
[Route("api/slots")]
[Produces("application/json")]
public class SlotsController(
    ISlotRepository slotRepo,
    IBookingRepository bookingRepo,
    IEmailService emailService,
    IConfiguration configuration) : ControllerBase
{
    // GET /api/slots?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
    [HttpGet]
    [ProducesResponseType<IEnumerable<Slot>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSlotsAsync(
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        [FromQuery] string? email,
        CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(startDate, "yyyy-MM-dd", out var start) ||
            !DateOnly.TryParseExact(endDate, "yyyy-MM-dd", out var end))
            return BadRequest("Les dates doivent être au format YYYY-MM-DD.");

        if (end < start)
            return BadRequest("endDate doit être >= startDate.");

        var result = await slotRepo.GetSlotsAsync(start, end, email, ct);
        return Ok(result);
    }

    // GET /api/slots/{id}
    [HttpGet("{id:guid}", Name = nameof(GetSlotByIdAsync))]
    [ProducesResponseType<Slot>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlotByIdAsync(
        Guid id,
        [FromQuery] string? email,
        CancellationToken ct)
    {
        var slot = await slotRepo.GetSlotByIdAsync(id, email, ct);
        return slot is null ? NotFound() : Ok(slot);
    }

    // POST /api/slots/{id}/book
    [HttpPost("{id:guid}/book")]
    [ProducesResponseType<BookSlotResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookSlotAsync(
        Guid id,
        [FromBody] BookSlotRequest request,
        CancellationToken ct)
    {
        var slot = await slotRepo.GetSlotByIdAsync(id, request.Email, ct);
        if (slot is null)
            return NotFound($"Créneau {id} introuvable.");

        if (slot.Status == SlotStatus.Full)
            return Conflict("Ce créneau est complet.");

        if (await bookingRepo.ExistsAsync(id, request.Email, ct))
            return Conflict("Vous avez déjà réservé ce créneau.");

        var booking = await bookingRepo.CreateBookingAsync(id, request.UserName, request.Email, ct);

        // Construire l'URL d'annulation et envoyer l'email de confirmation
        var frontendUrl = configuration["Frontend:Url"] ?? "https://ecole-marie-marvingt.github.io/planner";
        booking.CancellationUrl = $"{frontendUrl.TrimEnd('/')}?cancel={booking.CancellationToken}";

        var updatedSlot = await slotRepo.GetSlotByIdAsync(id, request.Email, ct);

        // Envoi en tâche de fond pour ne pas bloquer la réponse
        _ = emailService.SendBookingConfirmationAsync(booking, updatedSlot!, CancellationToken.None);

        return CreatedAtRoute(
            nameof(GetSlotByIdAsync),
            new { id },
            new BookSlotResponse(booking, updatedSlot!));
    }

    // DELETE /api/slots/{id}/book/{bookingId}
    [HttpDelete("{id:guid}/book/{bookingId:guid}")]
    [ProducesResponseType<CancelBookingResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBookingAsync(
        Guid id,
        Guid bookingId,
        [FromQuery] string? email,
        CancellationToken ct)
    {
        var booking = await bookingRepo.GetBookingAsync(bookingId, ct);
        if (booking is null || booking.SlotId != id)
            return NotFound("Réservation introuvable.");

        await bookingRepo.DeleteBookingAsync(bookingId, ct);
        var updatedSlot = await slotRepo.GetSlotByIdAsync(id, email, ct);

        return Ok(new CancelBookingResponse(updatedSlot!));
    }
}
