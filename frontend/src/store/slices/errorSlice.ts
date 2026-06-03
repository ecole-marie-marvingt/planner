import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export interface ErrorState {
  message: string | null;
  id: string;
}

const initialState: ErrorState = {
  message: null,
  id: '',
};

const errorSlice = createSlice({
  name: 'error',
  initialState,
  reducers: {
    setError(state, action: PayloadAction<string>) {
      state.message = action.payload;
      state.id = `${Date.now()}-${Math.random()}`;
    },
    clearError(state) {
      state.message = null;
      state.id = '';
    },
  },
});

export const { setError, clearError } = errorSlice.actions;
export default errorSlice.reducer;
