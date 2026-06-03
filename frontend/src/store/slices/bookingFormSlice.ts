import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { BookingFormState, Slot } from '../../types';
import { bookSlot, cancelBooking } from './slotsSlice';

const initialState: BookingFormState = {
  isOpen: false,
  selectedSlot: null,
  userName: '',
  email: '',
  phoneNumber: '',
  isSubmitting: false,
  error: null,
  successMessage: null,
};

const bookingFormSlice = createSlice({
  name: 'bookingForm',
  initialState,
  reducers: {
    openBookingModal(state, action: PayloadAction<Slot>) {
      state.isOpen = true;
      state.selectedSlot = action.payload;
      state.userName = '';
      state.email = '';
      state.phoneNumber = '';
      state.error = null;
      state.successMessage = null;
    },
    closeBookingModal(state) {
      state.isOpen = false;
      state.selectedSlot = null;
      state.error = null;
      state.successMessage = null;
    },
    setUserName(state, action: PayloadAction<string>) {
      state.userName = action.payload;
    },
    setEmail(state, action: PayloadAction<string>) {
      state.email = action.payload;
    },
    setPhoneNumber(state, action: PayloadAction<string>) {
      state.phoneNumber = action.payload;
    },
    clearMessages(state) {
      state.error = null;
      state.successMessage = null;
    },
  },
  extraReducers: (builder) => {
    // bookSlot
    builder.addCase(bookSlot.pending, (state) => {
      state.isSubmitting = true;
      state.error = null;
      state.successMessage = null;
    });
    builder.addCase(bookSlot.fulfilled, (state, action) => {
      state.isSubmitting = false;
      state.successMessage = `Réservation confirmée ! Un email de confirmation avec un lien d'annulation vous a été envoyé à ${action.payload.booking.email}.`;
      state.selectedSlot = action.payload.slot;
    });
    builder.addCase(bookSlot.rejected, (state, action) => {
      state.isSubmitting = false;
      state.error = (action.payload as string) ?? 'Erreur lors de la réservation';
    });

    // cancelBooking
    builder.addCase(cancelBooking.pending, (state) => {
      state.isSubmitting = true;
      state.error = null;
    });
    builder.addCase(cancelBooking.fulfilled, (state, action) => {
      state.isSubmitting = false;
      state.successMessage = 'Réservation annulée.';
      state.selectedSlot = action.payload.slot;
    });
    builder.addCase(cancelBooking.rejected, (state, action) => {
      state.isSubmitting = false;
      state.error = (action.payload as string) ?? "Erreur lors de l'annulation";
    });
  },
});

export const {
  openBookingModal,
  closeBookingModal,
  setUserName,
  setEmail,
  setPhoneNumber,
  clearMessages,
} = bookingFormSlice.actions;

export default bookingFormSlice.reducer;
