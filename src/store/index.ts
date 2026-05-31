import { configureStore } from '@reduxjs/toolkit';
import calendarReducer from './slices/calendarSlice';
import slotsReducer from './slices/slotsSlice';
import bookingFormReducer from './slices/bookingFormSlice';

export const store = configureStore({
  reducer: {
    calendar: calendarReducer,
    slots: slotsReducer,
    bookingForm: bookingFormReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
