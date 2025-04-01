import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { IBuildingBoundaries } from "../models/IBuildingBoundaries";
import { Address } from "@shared/types/ValueObjectsTypes";

const mapApi = createApi({
    reducerPath: "mapApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: BASE_URL
    }),
    endpoints: (build) => ({
        fetchBuildingBoundaries: build.query<IBuildingBoundaries, Address>({
            query: (address) => ({
                url: "/map/building-boundaries",
                params: {
                    address: `${address.city}, ${address.street}, ${address.houseNumber}`
                }
            })
        })
    }) 
});

export default mapApi;
