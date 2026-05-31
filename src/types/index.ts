// ─── Modèles ─────────────────────────────────────────────────────────────────

export type SlotStatus = 'available' | 'booked' | 'full';

export interface Slot {
  id: string;
  date: string;        // YYYY-MM-DD
  startTime: string;   // HH:mm
  endTime: string;     // HH:mm
  title: string;
  description?: string;
  capacity: number;
  bookedCount: number;
  status: SlotStatus;
  isBookedByMe: boolean;
  bookingId?: string;  // présent si isBookedByMe
}

export interface Booking {
  bookingId: string;
  slotId: string;
  userName: string;
  email: string;
  bookedAt: string; // ISO 8601
}

// ─── Requêtes / Réponses API ──────────────────────────────────────────────────

export interface GetSlotsParams {
  startDate: string; // YYYY-MM-DD
  endDate: string;   // YYYY-MM-DD
}

export interface BookSlotRequest {
  userName: string;
  email: string;
}

export interface BookSlotResponse {
  booking: Booking;
  slot: Slot;
}

export interface CancelBookingResponse {
  slot: Slot;
}

// ─── État Redux ───────────────────────────────────────────────────────────────

export interface CalendarState {
  currentMonth: number; // 0-indexed
  currentYear: number;
  selectedDate: string | null; // YYYY-MM-DD
}

export interface SlotsState {
  byDate: Record<string, Slot[]>; // clé = YYYY-MM-DD
  loadingDates: string[];
  errorByDate: Record<string, string>;
}

export interface BookingFormState {
  isOpen: boolean;
  selectedSlot: Slot | null;
  userName: string;
  email: string;
  isSubmitting: boolean;
  error: string | null;
  successMessage: string | null;
}
