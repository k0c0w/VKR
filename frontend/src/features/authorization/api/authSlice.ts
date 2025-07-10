import { User } from "@entities/user/models/User";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
    currentUser: User | undefined;
    sessionInfo: {
        p1: string;
        p2: string;
        p_h: string;
    } | undefined;
}

type AuthenticatedState = {
    currentUser: User;
    sessionInfo: {
        p1: string;
        p2: string;
        p_h: string;
    };
}

const initialState: AuthState = {
    currentUser: undefined,
    sessionInfo: undefined,
};

const authSlice = createSlice({
    name: "authorization",
    initialState,
    reducers: {
        authenticateUser(state, {payload}:PayloadAction<AuthenticatedState>) {
            const {currentUser, sessionInfo} = payload;
            state.currentUser = currentUser;
            state.sessionInfo = sessionInfo;

            sessionStorage.setItem('p1', sessionInfo.p1);
            sessionStorage.setItem('p2', sessionInfo.p2);
            sessionStorage.setItem('p_h', sessionInfo.p_h);
        },
        unauthenticateUser(state) {
            state.currentUser = undefined;
            state.sessionInfo = undefined;

            sessionStorage.removeItem('p1');
            sessionStorage.removeItem('p2');
            sessionStorage.removeItem('p_h');
        }
    }
})

export default authSlice.reducer;
export const { authenticateUser, unauthenticateUser } = authSlice.actions;