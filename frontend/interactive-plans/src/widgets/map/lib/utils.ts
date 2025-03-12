import { LatLng, LatLngExpression, LatLngLiteral, LatLngTuple } from "leaflet";

export function convertToLatLng(expr: LatLngExpression): LatLng {
    if ((expr as LatLng).lat !== undefined){
        return expr as LatLng;
    } else if ((expr as LatLngLiteral).lat !== undefined){
        const lt = expr as LatLngLiteral;

        return new LatLng(lt.lat, lt.lng);
    } 
    
    const tuple = expr as LatLngTuple;
    return new LatLng(tuple[0], tuple[1]);
}