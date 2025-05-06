import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { IBuildingBoundariesQuery, IBuildingBoundariesResponse } from "../models/BuildingBoundaries";
import { ICreateNewPlanArgs, ICreateNePlanRsult } from "../models/CreateNewPlan";

const mapApi = createApi({
    reducerPath: "mapApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: BASE_URL
    }),
    tagTypes: ['plans-list'],
    endpoints: (build) => ({
        fetchBuildingBoundaries: build.query<IBuildingBoundariesResponse, IBuildingBoundariesQuery>({
            query: (params) => ({
                url: "/map/building-boundaries",
                params: params,
            })
        }),
        createNewPlan: build.mutation<ICreateNePlanRsult, ICreateNewPlanArgs>({
            query: ({ building }) => ({
                url: 'plan',
                method: 'POST',
                body: building,
              }),
            invalidatesTags: ['plans-list'],
        })
    }) 
});

export default mapApi;
