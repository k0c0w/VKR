import { Marker, PM, Polygon, Polyline } from "leaflet";
import { Feature, FeatureCollection, Polygon as GeoJsonPolygon, LineString, Point, Position } from "geojson";
import L from "leaflet";
import * as turf from "@turf/turf";
import { getLayerLeafletId, isLatLangArray, isMarkerLayer, isPolygonLayer, isPolylineLayer } from "@shared/map/lib/leafletUtilsAdditions";
import { isWall, Room, Wall } from "@entities/map";
import { GEOJSON_VERTEX_COMPARISION_TOLERANCE, TO_GEOJSON_PRECISION } from "@app/config/constants";
import { isLineString, isPoint, isPolygon } from "@shared/types/geoJsonTypeGuards";
import alwaysTrue from "@shared/utils/alwaysTrue";
import { FeatureWithId, PointObject } from "@entities/map/common";


function hasSelfIntersection(feature: Feature<LineString | GeoJsonPolygon>) {
    return turf.kinks(feature).features.length > 0;
}

function isInsideBounds(feature: Feature<LineString | Point | GeoJsonPolygon, any>, bounds: GeoJsonPolygon) {
    switch(feature.geometry.type) {
        case "Point":
            return  turf.booleanPointInPolygon(feature as Feature<Point>, bounds);
        case "LineString":
            return isLineInBounds(feature as Feature<LineString, any>, bounds);
        case "Polygon":
            return turf.booleanContains(bounds, feature);
        default:
            throw new Error("Unsupported Feature geometry type.");
    }
}

function doesIntersectLevelFeatures(targetFeature: FeatureWithId<LineString | GeoJsonPolygon | Point, any>, other: FeatureWithId<LineString | GeoJsonPolygon>[]): boolean {
    const {geometry:targetGeometry} = targetFeature;
    for (const otherFeature of other) {
        
        if(targetFeature.id === otherFeature.id){
            // Skip self since always intersects by self.
            continue;
        } else if (targetGeometry.type === 'LineString' && isLineString(otherFeature)) {
            if (isCorrectLineRelation(targetFeature as Feature<LineString>, otherFeature)) {
                continue;
            }
            return true;
        } else if (targetGeometry.type === 'Polygon' && isPolygon(otherFeature)) {
            if (isCorrectPolygonRelation(targetFeature as Feature<GeoJsonPolygon>, otherFeature)) {
                continue;
            }
            return true;
        } else if (targetGeometry.type === "Point" && isLineString(otherFeature)) {
            return turf.booleanIntersects(targetFeature, otherFeature);
        }
    }
    return false;
}

function isCorrectLineRelation(targetFeature: Feature<LineString>, other: Feature<LineString>): boolean {
    const intersections = turf.lineIntersect(targetFeature, other);
    return intersections.features.length === 0 || allIntersectionsAreLineVertexes(intersections, targetFeature.geometry.coordinates);
}

function isCorrectPolygonRelation(targetFeature: Feature<GeoJsonPolygon>, other: Feature<GeoJsonPolygon>): boolean {
    const targetLine = turf.polygonToLine(targetFeature) as Feature<LineString>;
    const levelLine = turf.polygonToLine(other)
    const intersections = turf.lineIntersect(targetLine, levelLine);
    const noOrValidIntersection = (intersections.features.length === 0 
            && !turf.booleanContains(targetFeature, other) 
            && !turf.booleanContains(other, targetFeature))
        || allIntersectionsAreLineVertexes(intersections, targetLine.geometry.coordinates);
    return noOrValidIntersection;
}

function isLineInBounds(lineFeature: Feature<LineString, any>, bounds: GeoJsonPolygon): boolean {
    for(let i = 0; i < lineFeature.geometry.coordinates.length; i++) {
        const position = lineFeature.geometry.coordinates[i] as Position;
        if (!turf.booleanPointInPolygon(turf.point(position), bounds)) {
            return false;
        }
    }   
    const basePolyHasHoles = bounds.coordinates.length > 1;
    if (basePolyHasHoles) {
        for(let i = 1; i < bounds.coordinates.length; i++) {
            const holeCoordinates = bounds.coordinates[i];
            const intersection = turf.lineIntersect(lineFeature, turf.polygon([holeCoordinates]));
            if (intersection.features.length >= 1) {
                return false;
            }
        }
    }
    return true;
}

function allIntersectionsAreLineVertexes(intersections:  FeatureCollection<Point>, currentFeatureVertecies: Position[]): boolean {
    for (const {geometry} of intersections.features) {
        const [intersetionLng, intersectionLat] = geometry.coordinates;
        const intersectionIsVertexOnLine = currentFeatureVertecies
            .some(([vertexLng, vertexLat]) => Math.abs(vertexLng - intersetionLng) < GEOJSON_VERTEX_COMPARISION_TOLERANCE 
                && Math.abs(vertexLat - intersectionLat) < GEOJSON_VERTEX_COMPARISION_TOLERANCE);
        if (!intersectionIsVertexOnLine) {
            return false;
        }
    }
    return true;
}

function isValidLineStringPosition(feature: FeatureWithId<LineString>, bounds: GeoJsonPolygon, other: FeatureWithId<LineString>[]): boolean {
    if (hasSelfIntersection(feature)) {
        return false;
    }

    for(const otherFeature of other) {
        if (otherFeature.id === feature.id) {
            continue;
        } else if (!isCorrectLineRelation(feature, otherFeature)) {
            return false;
        }
    }

    return true;
}

function isValidPolygonPosition(feature: FeatureWithId<GeoJsonPolygon>, bounds: GeoJsonPolygon, other: FeatureWithId<GeoJsonPolygon>[]): boolean {
    if (hasSelfIntersection(feature)) {
        return false;
    }

    for(const levelFeature of other) {
        if (levelFeature.id === feature.id) {
            continue;
        } else if (!isCorrectPolygonRelation(feature, levelFeature)) {
            return false;
        }
    }

    return true;
}


export function isValidFeaturePosition({feature, bounds, lineStrings, polygons}:{feature: FeatureWithId<LineString | GeoJsonPolygon | Point>; bounds: GeoJsonPolygon; lineStrings:FeatureWithId<LineString>[]; polygons: FeatureWithId<GeoJsonPolygon>[]}): boolean {
    if (!isInsideBounds(feature, bounds)) {
        return false;
    }
    
    if(isLineString(feature)) {
        return isValidLineStringPosition(feature, bounds, lineStrings);
    } else if (isPolygon(feature)) {
        return isValidPolygonPosition(feature, bounds, polygons);
    } else if (isPoint(feature)) {
        return isInsideBounds(feature, bounds) && !doesIntersectLevelFeatures(feature, lineStrings); 
    }

    return true;
}
