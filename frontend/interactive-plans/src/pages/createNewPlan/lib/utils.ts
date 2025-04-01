import { Building } from "@entities/map/Building";
import { LatLngExpression } from "leaflet";

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
function getMapCenterByBuilding({boundaries}: Building): LatLngExpression {
    
    bounds = buidling.boundaries.coordinates;
    area = getClosedNonOverlappedPolygonArea()
}