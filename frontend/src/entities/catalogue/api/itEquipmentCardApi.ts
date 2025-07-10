import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"

export const itEquipmentCardApi = createApi({
  reducerPath: 'itEquipmentCardApi',
  baseQuery: fetchBaseQuery({
    prepareHeaders: (headers) => {
      headers.set('Accept', 'text/html');
      return headers;
    },
  }),
  endpoints: (builder) => ({
    fetchCardHtml: builder.query({
      query: (url) => ({
        url: `${url}&p1=${sessionStorage.getItem("p1")}&p2=${sessionStorage.getItem("p2")}`,
        responseHandler: async (response) => {
          if (!response.ok) {
            return { status: response.status, error: await response.text() };
          }

          const buffer = await response.arrayBuffer();
          const contentType = response.headers.get('Content-Type') || '';
          const charsetMatch = contentType.match(/charset=([\w-]+)/i);
          const charset = charsetMatch ? charsetMatch[1].toLowerCase() : 'utf-8';
          const decoder = new TextDecoder(charset);
          const decodedText = decoder.decode(buffer);
          return { status: response.status, data: decodedText };
        },
      }),
    }),
  }),
});
