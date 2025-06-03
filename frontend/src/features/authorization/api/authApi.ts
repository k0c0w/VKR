import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { ISignInArgs, ISignInResult } from "../models/SignIn";

const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/authorization`,
    }),
    endpoints: (build) => ({
        signIn: build.mutation<ISignInResult, ISignInArgs>({
            query: ({login, password}) => ({
                url: "/sign-in",
                params: {
                    email: login,
                    password: password,
                },
                method: 'POST'
            })
        }),
        signOut: build.mutation<void, void>({
            query: () => ({
                url: '/sign-out',
                method: 'POST'
            })
        })
    }) 
});

export default authApi;
