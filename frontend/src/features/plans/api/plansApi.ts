import { BASE_URL } from "@app/config/env";
import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { ICreateNewPlanResult, ICreateNewPlanArgs } from "../models/CreateNewPlan";
import { IGetPlanListResult } from "../models/GetPlanList";
import { ISpecificPlan, ISpecificPlanArgs } from "../models/SpecificPlan";
import { IRequestPlanRemovalArgs } from "../models/RequestRemoval";
import { IUpdatePlanArg } from "../models/UpdatePlan";

const plansApi = createApi({
    reducerPath: "plansApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/plans`,
        credentials: 'include'
    }),
    tagTypes: ['plans-list', 'concrette-plan'],
    endpoints: (build) => ({
        getAvailablePlansList: build.query<IGetPlanListResult[], void>({
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
            }),
            providesTags: (result, _, { id }) => result ? [{ type: 'concrette-plan', id }] : [],
        }),
        createNewPlan: build.mutation<ICreateNewPlanResult, ICreateNewPlanArgs>({
            query: (building) => ({
                url: '/',
                method: 'POST',
                body: building,
              }),
            invalidatesTags: ['plans-list'],
        }),
        requestPlanRemoval: build.mutation<any, IRequestPlanRemovalArgs>({
            query: ({buildingId}) => ({
                url: `/${buildingId}`,
                method: 'DELETE'
            }),
            invalidatesTags: ['plans-list'],
        }),
        updatePlan: build.mutation<ISpecificPlan, IUpdatePlanArg>({
            query: ({id, updateInstructions }) => ({
                url: `/${id}`,
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json'   
                },
                body: JSON.stringify(updateInstructions)
            }),
            invalidatesTags: (result, _, { id }) => result ? [{ type: 'concrette-plan', id }] : [],
        })
    }) 
});

export default plansApi;