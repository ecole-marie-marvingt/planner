import {
  createSlice,
  createAsyncThunk,
  type PayloadAction,
} from '@reduxjs/toolkit';
import { slotsApi } from '../../api/slotsApi';
import type { Slot, SlotsState, BookSlotRequest } from '../../types';
import { setError } from './errorSlice';

// ─── Thunks ───────────────────────────────────────────────────────────────────

export const fetchSlotsForRange = createAsyncThunk(
  'slots/fetchForRange',
  async (
    { startDate, endDate }: { startDate: string; endDate: string },
    { rejectWithValue, dispatch }
  ) => {
    try {
      const slots = await slotsApi.getSlots({ startDate, endDate });
      return slots;
    } catch (err) {
      const errorMsg = (err as Error).message;
      dispatch(setError(errorMsg));
      return rejectWithValue(errorMsg);
    }
  }
);

export const bookSlot = createAsyncThunk(
  'slots/book',
  async (
    { slotId, payload }: { slotId: string; payload: BookSlotRequest },
    { rejectWithValue, dispatch }
  ) => {
    try {
      const response = await slotsApi.bookSlot(slotId, payload);
      return response;
    } catch (err) {
      const errorMsg = (err as Error).message;
      dispatch(setError(errorMsg));
      return rejectWithValue(errorMsg);
    }
  }
);

export const cancelBooking = createAsyncThunk(
  'slots/cancel',
  async (
    { slotId, bookingId }: { slotId: string; bookingId: string },
    { rejectWithValue, dispatch }
  ) => {
    try {
      const response = await slotsApi.cancelBooking(slotId, bookingId);
      return response;
    } catch (err) {
      const errorMsg = (err as Error).message;
      dispatch(setError(errorMsg));
      return rejectWithValue(errorMsg);
    }
  }
);

// ─── Slice ────────────────────────────────────────────────────────────────────

const initialState: SlotsState = {
  byDate: {},
  loadingDates: [],
  errorByDate: {},
};

/** Regroupe une liste de slots par date */
function groupByDate(slots: Slot[]): Record<string, Slot[]> {
  return slots.reduce<Record<string, Slot[]>>((acc, slot) => {
    (acc[slot.date] ??= []).push(slot);
    return acc;
  }, {});
}

const slotsSlice = createSlice({
  name: 'slots',
  initialState,
  reducers: {
    updateSlotInStore(state, action: PayloadAction<Slot>) {
      const slot = action.payload;
      const list = state.byDate[slot.date];
      if (list) {
        const idx = list.findIndex((s) => s.id === slot.id);
        if (idx !== -1) list[idx] = slot;
      }
    },
  },
  extraReducers: (builder) => {
    // fetchSlotsForRange
    builder.addCase(fetchSlotsForRange.pending, (state, action) => {
      // Marque les dates comme en cours de chargement
      const { startDate, endDate } = action.meta.arg;
      if (!state.loadingDates.includes(startDate))
        state.loadingDates.push(startDate);
      if (!state.loadingDates.includes(endDate))
        state.loadingDates.push(endDate);
    });
    builder.addCase(fetchSlotsForRange.fulfilled, (state, action) => {
      const grouped = groupByDate(action.payload);
      state.byDate = { ...state.byDate, ...grouped };
      const { startDate, endDate } = action.meta.arg;
      state.loadingDates = state.loadingDates.filter(
        (d) => d !== startDate && d !== endDate
      );
    });
    builder.addCase(fetchSlotsForRange.rejected, (state, action) => {
      const { startDate, endDate } = action.meta.arg;
      const msg = (action.payload as string) ?? 'Erreur';
      state.errorByDate[startDate] = msg;
      state.errorByDate[endDate] = msg;
      state.loadingDates = state.loadingDates.filter(
        (d) => d !== startDate && d !== endDate
      );
    });

    // bookSlot
    builder.addCase(bookSlot.fulfilled, (state, action) => {
      const { slot } = action.payload;
      const list = state.byDate[slot.date];
      if (list) {
        const idx = list.findIndex((s) => s.id === slot.id);
        if (idx !== -1) list[idx] = slot;
      }
    });

    // cancelBooking
    builder.addCase(cancelBooking.fulfilled, (state, action) => {
      const { slot } = action.payload;
      const list = state.byDate[slot.date];
      if (list) {
        const idx = list.findIndex((s) => s.id === slot.id);
        if (idx !== -1) list[idx] = slot;
      }
    });
  },
});

export const { updateSlotInStore } = slotsSlice.actions;
export default slotsSlice.reducer;
