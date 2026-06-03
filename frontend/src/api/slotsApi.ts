/**
 * Contrat de l'API REST :
 *
 * GET    /api/slots?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD   → Slot[]
 * GET    /api/slots/:id                                        → Slot
 * POST   /api/slots/:id/book                                   → BookSlotResponse
 *          body: { userName, email, phoneNumber }
 * DELETE /api/slots/:id/book/:bookingId                        → CancelBookingResponse
 * GET    /api/bookings/cancel/:cancellationToken               → CancelBookingResponse
 */

import axios from 'axios';
import type {
  Slot,
  GetSlotsParams,
  BookSlotRequest,
  BookSlotResponse,
  CancelBookingResponse,
} from '../types';

const api = axios.create({
  baseURL: import.meta.env.PROD ? import.meta.env.VITE_API_BASE_URL : null,
  headers: { 'Content-Type': 'application/json' },
  timeout: 10_000,
});

// Intercepteur global pour les erreurs
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message: string =
      error.response?.data?.message ?? error.message ?? 'Erreur inconnue';
    return Promise.reject(new Error(message));
  }
);

export const slotsApi = {
  /**
   * Récupère tous les créneaux entre deux dates incluses.
   */
  getSlots: async (params: GetSlotsParams): Promise<Slot[]> => {
    const { data } = await api.get<Slot[]>('/api/slots', { params });
    return data;
  },

  /**
   * Récupère un créneau par son identifiant.
   */
  getSlotById: async (id: string): Promise<Slot> => {
    const { data } = await api.get<Slot>(`/api/slots/${id}`);
    return data;
  },

  /**
   * Réserve un créneau pour un utilisateur.
   */
  bookSlot: async (
    slotId: string,
    payload: BookSlotRequest
  ): Promise<BookSlotResponse> => {
    const { data } = await api.post<BookSlotResponse>(
      `/api/slots/${slotId}/book`,
      payload
    );
    return data;
  },

  /**
   * Annule une réservation existante.
   */
  cancelBooking: async (
    slotId: string,
    bookingId: string
  ): Promise<CancelBookingResponse> => {
    const { data } = await api.delete<CancelBookingResponse>(
      `/api/slots/${slotId}/book/${bookingId}`
    );
    return data;
  },

  /**
   * Annule une réservation via le token contenu dans l'email de confirmation.
   */
  cancelByToken: async (cancellationToken: string): Promise<CancelBookingResponse> => {
    const { data } = await api.get<CancelBookingResponse>(
      `/api/bookings/cancel/${cancellationToken}`
    );
    return data;
  },
};
