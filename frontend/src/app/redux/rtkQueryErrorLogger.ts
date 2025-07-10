import { isRejectedWithValue } from '@reduxjs/toolkit'
import type { Middleware } from '@reduxjs/toolkit'


export const rtkQueryErrorLogger: Middleware =
  (_) => (next) => (action) => {
    // RTK Query uses `createAsyncThunk` from redux-toolkit under the hood, so we're able to utilize these matchers!
    if (isRejectedWithValue(action)) {
        const message =
        'data' in action.error
          ? (action.error.data as { message: string }).message
          : action.error.message;
        
        console.warn('Rejected action', action, message);
    }

    return next(action)
  }