import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { ISignInArgs } from "../models/SignIn";

const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/authorization`
    }),
    endpoints: (build) => ({
        signIn: build.mutation<any, ISignInArgs>({
            query: ({login, password, persist}) => ({
                url: "/sign-in",
                params: {
                    login: login,
                    password: password,
                    persist: persist,
                },
                method: 'POST'
            })
        }),
        signOut: build.mutation<any, any>({
            query: () => ({
                url: '/sign-out',
                method: 'POST'
            })
        })
    }) 
});

export default authApi;
