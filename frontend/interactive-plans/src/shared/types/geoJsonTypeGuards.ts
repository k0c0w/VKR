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