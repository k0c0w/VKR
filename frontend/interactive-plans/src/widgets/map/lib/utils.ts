import { FitBoundsOptions, LatLngLiteral, LatLngTuple } from "leaflet";
import { Polygon as GeoJsonPolygon } from "geojson";

function getClosedNonOverlappedPolygonArea(simplePolygon: number[][]): number {
    let sum = 0;
    let xi: number, xiplus1: number;
    let yi: number, yiplus1: number;

    for(let i = 0; i < simplePolygon.length - 1; i++) {
        [xi, yi] = simplePolygon[i];
        [xiplus1, yiplus1] = simplePolygon[i+1];

        sum += xi * yiplus1 - xiplus1 * yi;
    }

    return 0.5 * sum;
}

// Shoelace formula
/** @deprecated */
function getMapCenterByBuilding({coordinates, type}: GeoJsonPolygon): LatLngTuple {
    if (type !== "Polygon") {
        throw new Error(`{Unsupported Polygon type: ${type}`)
    }

    let outerPolygon = coordinates[0];
    let exteriorBoundsArea = getClosedNonOverlappedPolygonArea(outerPolygon);

    for(let i = 1; i < coordinates.length; i++) {
        let interiorBoundsArea = getClosedNonOverlappedPolygonArea(coordinates[i]);
        exteriorBoundsArea -= interiorBoundsArea;
    }

    if (exteriorBoundsArea <= 0) {
        exteriorBoundsArea = 1;
    }

    let centroidX = 0;
    let centroidY = 0;
    let coef: number;

    for(let i = 0; i < outerPolygon.length - 1; i++) {
        let [xi, yi] = outerPolygon[i];
        let [xiplus1, yiplus1] = outerPolygon[i+1];
        coef = xi * yiplus1 - xiplus1 * yi;

        centroidX += (xi + xiplus1) * coef;
        centroidY += (yi + yiplus1) * coef;
    }

    let divisionCoef: number = 6 * exteriorBoundsArea;
    
    return [centroidX / divisionCoef, centroidY / divisionCoef];
}

/** @deprecated */
function findBoundingBox({coordinates}: GeoJsonPolygon) {
    let minX = Infinity, minY = Infinity;
    let maxX = -Infinity, maxY = -Infinity;

    const exteriorBounds = coordinates[0];
    for (const [x, y] of exteriorBounds) {
        if (x < minX) minX = x;
        if (y < minY) minY = y;
        if (x > maxX) maxX = x;
        if (y > maxY) maxY = y;
    }

    const topLeft: [number, number] = [minX, minY]; 
    const bottomRight: [number, number] = [maxX, maxY];

    return { topLeft, bottomRight };
}

/** @deprecated use Polygon.getBounds() instead */
function getFitBoundOptions(boundaries: GeoJsonPolygon): FitBoundsOptions {
    const { topLeft, bottomRight } = findBoundingBox(boundaries);

    return {
        paddingTopLeft: topLeft,
        paddingBottomRight: bottomRight,
    };
}

function mapGeoJsonPolygonToLeafletExpression(polygon: GeoJsonPolygon): LatLngLiteral[][] {
    const latLngExpr = polygon.coordinates
            .map(linearRing => linearRing
                .map(coordinate => ({
                    lat: coordinate[0],
                    lng: coordinate[1]
                }))
            );
    
    return latLngExpr;
}

export { getFitBoundOptions, mapGeoJsonPolygonToLeafletExpression, getMapCenterByBuilding };