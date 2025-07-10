import guid from "@shared/types/guid";
import { Feature, GeoJsonProperties, Geometry, Point } from "geojson";


export interface PointObject<TId extends guid | number> extends FeatureWithId<TId, Point> {}

export interface FeatureWithId<TId extends guid | number, G extends Geometry | null = Geometry, P = GeoJsonProperties> extends Feature<G, P> {
    id:TId;
}
