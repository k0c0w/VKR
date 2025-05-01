import { FeatureWithId, isRoom, isWall, Room, RoomType, Wall } from "@entities/map";
import { isKnownShapeLayer, isPolygonLayer, isPolylineLayer, toGeoJsonWithId } from "@shared/map";
import { LayerWithFeatureId } from "@shared/map/lib/leafletTypeExtensions";
import { isLineString, isPolygon } from "@shared/types/geoJsonTypeGuards";
import { LineString, Polygon as GeoJsonPolygon } from "geojson";
import { Layer } from "leaflet";
import { audienceStyle, hallStyle, wallStyle } from "./geoman/styling";

        
export function splitWallsAndRooms(layers: {[layerId: number]: LayerWithFeatureId}) {
    const walls: FeatureWithId<LineString>[] = [];
    const rooms: FeatureWithId<GeoJsonPolygon>[] = [];
    
    Object.values(layers).forEach(layer => {
        if (isKnownShapeLayer(layer)) {
            const feature = toGeoJsonWithId(layer);
            if (isLineString(feature)) {
                walls.push(feature);
            } else if (isPolygon(feature)) {
                rooms.push(feature);
            }
        }
    });

    return {
        walls,
        rooms,
    }
}

export function resetStyle(layer: Layer, originalFeature: Wall | Room ) {
    if(isPolylineLayer(layer) && isWall(originalFeature)) {
        layer.setStyle(wallStyle);
    } else if(isPolygonLayer(layer) && isRoom(originalFeature)) {
        layer.setStyle(originalFeature.properties.type === RoomType.Audience ? audienceStyle : hallStyle);   
    }
}