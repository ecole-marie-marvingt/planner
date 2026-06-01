using Planner.Api.Models;

namespace Planner.Api.DTOs;

public sealed record BookSlotResponse(Booking Booking, Slot Slot);

public sealed record CancelBookingResponse(Slot Slot);
