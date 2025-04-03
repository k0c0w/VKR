import { LatLngBounds, LatLngLiteral, LatLngTuple } from "leaflet";
import { Polygon as GeoJsonPolygon, } from "geojson";

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

function getBounds({coordinates}: GeoJsonPolygon): LatLngBounds {
    const exteriorRing = coordinates[0] as LatLngTuple[];
    const latLngBounds = new LatLngBounds(exteriorRing);

    return latLngBounds;
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

const polygonType: 'Polygon' = 'Polygon';
function mapLeafletExpressionToGeoJsonPolygon(expr: LatLngLiteral[][]): GeoJsonPolygon {
    const coordinates = expr.map(ring => ring.map(point => [point.lat, point.lng]));

    return {
        type: polygonType,
        coordinates
    };
}

export { getBounds, getMapCenterByBuilding };
export { mapLeafletExpressionToGeoJsonPolygon, mapGeoJsonPolygonToLeafletExpression };