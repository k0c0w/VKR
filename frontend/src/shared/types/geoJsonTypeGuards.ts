import { Feature, Geometry, LineString, Polygon, Point } from "geojson";

export function isLineString(obj: Feature | Geometry ): obj is Feature<LineString> | LineString {
    if (obj.type === "Feature") {
        obj = obj.geometry;
    }

    return obj.type === "LineString";
}

export function isPolygon(obj: Feature | Geometry): obj is Feature<Polygon> | Polygon {
    if (obj.type === "Feature") {
        obj = obj.geometry;
    }

    return obj.type === "Polygon";
}

export function isPoint(obj: Feature | Geometry): obj is Feature<Point> | Point {
    if (obj.type === "Feature") {
        obj = obj.geometry;
    }

    return obj.type === "Point";
}

export function isFeature(obj: any): obj is Feature {
    return "type" in obj 
        && obj.type === "Feature"
        && "geometry" in obj
        && "type" in obj.geometry
        && (obj.geometry.type === "Point" || obj.geometry.type === "LineString" || obj.geometry.type === "Polygon")
        && "coordinates" in obj.geometry
        && Array.isArray(obj.geometry.coordinates);
}