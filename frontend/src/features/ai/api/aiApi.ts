import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { BASE_URL } from "@app/config/env";
import { IRequestPlanLabelingArgs, IRequestPlanLabelingResponse } from "../models/IRequestPlanLabeling";
import { IGetPlanLabelingResultArgs, IGetPlanLabelingResultResponse } from "../models/IGetPlanLabelingResult";

const aiApi = createApi({
    reducerPath: "aiApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/ai`,
        credentials: 'include'
    }),
    endpoints: (build) => ({
        requestPlanLabeling: build.mutation<IRequestPlanLabelingResponse, IRequestPlanLabelingArgs>({
            query: ({planImage}) => {
                const formData = new FormData();
                formData.append('planImage', planImage);

                return {
                    url: '/indoor-plans',
                    body: formData,
                    method: 'POST'
                }
            }
        }),
        getPlanLabelingResult: build.query<IGetPlanLabelingResultResponse, IGetPlanLabelingResultArgs>({
            query: ({requestId}) => ({
                url: `/indoor-plans/${requestId}`,
                method: 'GET',
                headers: {
                    'Cache-Control': 'no-cache'
                }
            }),
        }),
    }) 
});

export default aiApi;
