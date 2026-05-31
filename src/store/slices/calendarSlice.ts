import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { CalendarState } from '../../types';

const now = new Date();

const initialState: CalendarState = {
  currentMonth: now.getMonth(),
  currentYear: now.getFullYear(),
  selectedDate: null,
};

const calendarSlice = createSlice({
  name: 'calendar',
  initialState,
  reducers: {
    goToPreviousMonth(state) {
      if (state.currentMonth === 0) {
        state.currentMonth = 11;
        state.currentYear -= 1;
      } else {
        state.currentMonth -= 1;
      }
      state.selectedDate = null;
    },
    goToNextMonth(state) {
      if (state.currentMonth === 11) {
        state.currentMonth = 0;
        state.currentYear += 1;
      } else {
        state.currentMonth += 1;
      }
      state.selectedDate = null;
    },
    goToToday(state) {
      const today = new Date();
      state.currentMonth = today.getMonth();
      state.currentYear = today.getFullYear();
      state.selectedDate = today.toISOString().slice(0, 10);
    },
    selectDate(state, action: PayloadAction<string>) {
      state.selectedDate =
        state.selectedDate === action.payload ? null : action.payload;
    },
  },
});

export const { goToPreviousMonth, goToNextMonth, goToToday, selectDate } =
  calendarSlice.actions;

export default calendarSlice.reducer;
