import { Layer } from "leaflet";
import { Polygon, Feature } from "geojson";

declare module "leaflet" {
    interface Layer {
        toGeoJSON<G, P>(): Feature<G, P> /* {
            type: string;
            properties: any;
            geometry: Polygon;
        }*/
    }
}