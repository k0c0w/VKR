import { BASE_URL } from "@app/config/env";
import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { ICreateNewPlanResult, ICreateNewPlanArgs } from "../models/CreateNewPlan";
import { IGetPlanListResult } from "../models/GetPlanList";
import { ISpecificPlan, ISpecificPlanArgs } from "../models/SpecificPlan";

const plansApi = createApi({
    reducerPath: "plansApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/plans`
    }),
    tagTypes: ['plans-list'],
    endpoints: (build) => ({
        getAvailablePlansList: build.query<IGetPlanListResult, any>({
            query: () => ({
                url: '/',
                method: 'GET'
            }),
            providesTags: ['plans-list']
        }),
        getSpecificPlan: build.query<ISpecificPlan, ISpecificPlanArgs>({
            query: ({id}) => ({
                url: `/${id}`,
                method: 'GET'
            })
        }),
        createNewPlan: build.mutation<ICreateNewPlanResult, ICreateNewPlanArgs>({
            query: (building) => ({
                url: '/',
                method: 'POST',
                body: building,
              }),
            invalidatesTags: ['plans-list'],
        }),
    }) 
});

export default plansApi;