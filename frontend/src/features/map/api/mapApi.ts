import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { IBuildingBoundaries } from "../models/IBuildingBoundaries";
import { Address } from "@shared/types/ValueObjectsTypes";
import { ICreateNewPlanArgs, ICreateNePlanRsult } from "../models/CreateNewPlan";

const mapApi = createApi({
    reducerPath: "mapApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: BASE_URL
    }),
    tagTypes: ['plans-list'],
    endpoints: (build) => ({
        fetchBuildingBoundaries: build.query<IBuildingBoundaries, Address>({
            query: (address) => ({
                url: "/map/building-boundaries",
                params: {
                    city: address.city,
                    street: address.street,
                    houseNumber: address
                }
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
