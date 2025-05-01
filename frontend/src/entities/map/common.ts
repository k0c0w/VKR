import { guid } from "@shared/types/guid";
import { Feature, GeoJsonProperties, Geometry, Point } from "geojson";


export interface PointObject extends FeatureWithId<Point> {}

export interface FeatureWithId<G extends Geometry | null = Geometry, P = GeoJsonProperties> extends Feature<G, P> {
    id: guid;
}