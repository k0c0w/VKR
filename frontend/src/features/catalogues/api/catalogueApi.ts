import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { ItEquipmetCatalogue } from "../models/ItEquipment";
import { CatalogueBuilding } from "@widgets/editor/ui/RoomPopup";

const catalogueApi = createApi({
    reducerPath: "catalogueApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${BASE_URL}/catalogues`,
        credentials: 'include'
    }),
    endpoints: (build) => ({
        getRegions: build.query<string[], void>({
            query: () => ({
                url: 'regions',
                method: 'GET',
            }),
        }),
        getBuildings: build.query<CatalogueBuilding[], { region: string; }>({
            query: ({ region }) => ({
                url: 'buildings',
                method: 'GET',
                params: { 
                    region,
                },
            }),
        }),
        getBuilding: build.query<CatalogueBuilding, { region: string; name: string; address: string }>({
            query: ({ region, name, address }) => ({
                url: 'buildings',
                method: 'GET',
                params: { 
                    region,
                    ...(name && { name }),
                    ...(address && { address }),
                },
            }),
        }),
        getItEquipment: build.query<ItEquipmetCatalogue, { roomIds: number[] }>({
            query: ({ roomIds }) => {
                const queryString = roomIds.map((id) => `roomId=${id}`).join('&');
                return {
                    url: `it-equipment?${queryString}`,
                    method: 'GET',
                };
            },
        }),
    }) 
});

export default catalogueApi;