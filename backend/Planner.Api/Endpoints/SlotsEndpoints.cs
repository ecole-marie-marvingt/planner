using Microsoft.AspNetCore.Mvc;
using Planner.Api.DTOs;
using Planner.Api.Models;
using Planner.Api.Repositories;

namespace Planner.Api.Endpoints;

public static class SlotsEndpoints
{
    public static IEndpointRouteBuilder MapSlotsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/slots")
                       .WithTags("Slots");

        // GET /api/slots?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD
        group.MapGet("/", GetSlotsAsync)
             .WithName("GetSlots")
             .WithSummary("Récupère tous les créneaux entre deux dates")
             .Produces<IEnumerable<Slot>>();

        // GET /api/slots/{id}
        group.MapGet("/{id:guid}", GetSlotByIdAsync)
             .WithName("GetSlotById")
             .WithSummary("Récupère un créneau par son identifiant")
             .Produces<Slot>()
             .Produces(StatusCodes.Status404NotFound);

        // POST /api/slots/{id}/book
        group.MapPost("/{id:guid}/book", BookSlotAsync)
             .WithName("BookSlot")
             .WithSummary("Réserve un créneau")
             .Produces<BookSlotResponse>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status409Conflict);

        // DELETE /api/slots/{id}/book/{bookingId}
        group.MapDelete("/{id:guid}/book/{bookingId:guid}", CancelBookingAsync)
             .WithName("CancelBooking")
             .WithSummary("Annule une réservation")
             .Produces<CancelBookingResponse>()
             .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private static async Task<IResult> GetSlotsAsync(
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        [FromQuery] string? email,
        ISlotRepository slots,
        CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(startDate, "yyyy-MM-dd", out var start) ||
            !DateOnly.TryParseExact(endDate, "yyyy-MM-dd", out var end))
            return Results.BadRequest("Les dates doivent être au format YYYY-MM-DD.");

        if (end < start)
            return Results.BadRequest("endDate doit être >= startDate.");

        var result = await slots.GetSlotsAsync(start, end, email, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetSlotByIdAsync(
        Guid id,
        [FromQuery] string? email,
        ISlotRepository slots,
        CancellationToken ct)
    {
        var slot = await slots.GetSlotByIdAsync(id, email, ct);
        return slot is null ? Results.NotFound() : Results.Ok(slot);
    }

    private static async Task<IResult> BookSlotAsync(
        Guid id,
        [FromBody] BookSlotRequest request,
        ISlotRepository slotRepo,
        IBookingRepository bookingRepo,
        CancellationToken ct)
    {
        var slot = await slotRepo.GetSlotByIdAsync(id, request.Email, ct);
        if (slot is null)
            return Results.NotFound($"Créneau {id} introuvable.");

        if (slot.Status == SlotStatus.Full)
            return Results.Conflict("Ce créneau est complet.");

        if (await bookingRepo.ExistsAsync(id, request.Email, ct))
            return Results.Conflict("Vous avez déjà réservé ce créneau.");

        var booking = await bookingRepo.CreateBookingAsync(id, request.UserName, request.Email, ct);
        var updatedSlot = await slotRepo.GetSlotByIdAsync(id, request.Email, ct);

        return Results.Created(
            $"/api/slots/{id}",
            new BookSlotResponse(booking, updatedSlot!));
    }

    private static async Task<IResult> CancelBookingAsync(
        Guid id,
        Guid bookingId,
        [FromQuery] string? email,
        ISlotRepository slotRepo,
        IBookingRepository bookingRepo,
        CancellationToken ct)
    {
        var booking = await bookingRepo.GetBookingAsync(bookingId, ct);
        if (booking is null || booking.SlotId != id)
            return Results.NotFound("Réservation introuvable.");

        await bookingRepo.DeleteBookingAsync(bookingId, ct);
        var updatedSlot = await slotRepo.GetSlotByIdAsync(id, email, ct);

        return Results.Ok(new CancelBookingResponse(updatedSlot!));
    }
}
