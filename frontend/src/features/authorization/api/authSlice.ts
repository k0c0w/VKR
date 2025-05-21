import { User } from "@entities/user/User";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";

type AuthState = {
    currentUser: User | undefined;
}

const initialState: AuthState = {
    currentUser: undefined
}

const authSlice = createSlice({
    name: "authorization",
    initialState,
    reducers: {
        setUser(state, {payload}:PayloadAction<User|undefined>) {
            state.currentUser = payload;
        }
    }
})

export default authSlice.reducer;
export const { setUser } = authSlice.actions;