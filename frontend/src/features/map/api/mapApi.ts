import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { IBuildingGeometryQuery, IBuildingGeometryResponse } from "../models/BuildingBoundaries";

const mapApi = createApi({
    reducerPath: "mapApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/map`,
        credentials: 'include'
    }),
    endpoints: (build) => ({
        fetchBuildingBoundaries: build.query<{geometry: IBuildingGeometryResponse}, IBuildingGeometryQuery>({
            query: (params) => ({
                url: "/building-boundaries",
                params: params,
            })
        }),
    }) 
});

export default mapApi;
