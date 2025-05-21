import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { IBuildingBoundariesQuery, IBuildingBoundariesResponse } from "../models/BuildingBoundaries";

const mapApi = createApi({
    reducerPath: "mapApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/map`
    }),
    endpoints: (build) => ({
        fetchBuildingBoundaries: build.query<IBuildingBoundariesResponse, IBuildingBoundariesQuery>({
            query: (params) => ({
                url: "/building-boundaries",
                params: params,
            })
        }),
    }) 
});

export default mapApi;
