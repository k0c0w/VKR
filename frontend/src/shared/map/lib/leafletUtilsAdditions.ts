import { GeoJSON, LatLngBounds, LatLngLiteral, LatLngTuple, LatLngExpression, LatLng, Layer, Polyline, PM , Polygon, Marker} from "leaflet";
import { Feature, Polygon as GeoJsonPolygon, Geometry, LineString, Point, Position, } from "geojson";
import { GEOJSON_PRECISION } from "@app/config/constants";
import { booleanPointInPolygon } from "@turf/turf";
import { LayerWithFeatureId } from "./leafletTypeExtensions";
import { FeatureWithId } from "@entities/map";

function isLatLngLiteralOrLatLng(latLng: LatLngExpression): latLng is LatLngLiteral | LatLng {
    return (latLng as LatLngLiteral).lat !== undefined;
}

function isLatLngTuple(latLng: LatLngExpression): latLng is LatLngTuple {
    const tuple = (latLng as LatLngTuple);

    return tuple.length && (tuple.length === 2 || tuple.length === 3);
}

function isLatLangArray(latLngs: LatLng[] | LatLng[][] | LatLng[][][]): latLngs is LatLng[] {
    
    return (latLngs.length === 0) || (latLngs.length > 0 && ((latLngs as LatLng[][] | LatLng[][][])[0].length === undefined));
}

function isArrayOfLatLangArray(latLngs: LatLng[] | LatLng[][] | LatLng[][][]): latLngs is LatLng[][] {
    return (latLngs.length === 0) 
    || (latLngs.length > 0 
        && ((latLngs as LatLng[])[0].lat === undefined 
            && ((latLngs as LatLng[][])[0].length === 0
                || (latLngs as LatLng[][])[0][0].lat !== undefined 
            )
        )
    );
}

function getShape(layer: Layer): PM.SUPPORTED_SHAPES | undefined {
    // @ts-ignore
    return layer.pm?.getShape && typeof layer.pm.getShape === 'function' ? layer.pm.getShape() : undefined;
}

function isPolylineLayer(layer: Layer): layer is Polyline {
    return getShape(layer) === 'Line';
}

function isPolygonLayer(layer: Layer): layer is Polygon {
    return getShape(layer) === 'Polygon';
}

function isMarkerLayer(layer: Layer): layer is Marker {
    return getShape(layer) === 'Marker';
} 

function isKnownShapeLayer(layer: Layer): layer is Polygon | Polyline | Marker {
    return isPolygonLayer(layer) || isPolylineLayer(layer) || isMarkerLayer(layer);
}

function getBounds({coordinates}: GeoJsonPolygon): LatLngBounds {
    const exteriorRing = coordinates[0] as LatLngTuple[];
    const latLngBounds = new LatLngBounds(exteriorRing);

    return latLngBounds;
}

function getLayerLeafletId(layer: Layer): number {
    // @ts-ignore
    const id: number | undefined = layer._leaflet_id;
    if (id) {
        return id as number;
    }

    throw new Error("Could not get Layer Leaflet Id.", {
        cause: layer
    });
}

function setLayerLeafletId(layer: Layer, leaflet_id: number) {
    // @ts-ignore
    layer._leaflet_id = leaflet_id;
}

function mapGeoJsonPolygonToLeafletExpression(polygon: GeoJsonPolygon): LatLngExpression[][] {
    return GeoJSON.coordsToLatLngs(polygon.coordinates, 1);
}

function mapGeoJsonLineStringToLeafletExpression(line: LineString): LatLngExpression[] {
    return GeoJSON.coordsToLatLngs(line.coordinates);
}

function mapLeafletExpressionToGeoJsonPolygon(expr: LatLngExpression[][]): GeoJsonPolygon {
    const coordinates = expr.map(ring => ring.map(point => {
        if (isLatLngLiteralOrLatLng(point)) {
            return [point.lng, point.lat] as Position;
        } else if (isLatLngTuple(point)) {
            return [...point].reverse() as Position;
        }

        throw new Error("Unsupported LatLngExpression!");
    }
 ));

    return {
        type: 'Polygon',
        coordinates
    };
}

export function toGeoJsonWithId(layer: LayerWithFeatureId): FeatureWithId<LineString | GeoJsonPolygon | Point> {
    if (!isKnownShapeLayer(layer)) {
        throw new Error("Unsupported layer. Can not retrieve geometry.", {
            cause: layer
        });
    }

    const feature = layer.toGeoJSON(GEOJSON_PRECISION);
    feature.id = layer.featureId;
    feature.geometry = roundCoordinates(feature.geometry, GEOJSON_PRECISION);

    return feature as FeatureWithId<LineString | GeoJsonPolygon | Point>;
}

function getGeoJsonFeatureGeometryFrom(layer:Layer) {
    if (!isKnownShapeLayer(layer)) {
        throw new Error("Unsupported layer. Can not retrieve geometry.", {
            cause: layer
        });
    }

    const { geometry } = layer.toGeoJSON(GEOJSON_PRECISION);
    return geometry;
}

function findPolygonContainingPoint<TPoly extends Feature<GeoJsonPolygon>, TPoint extends Feature<Point>>(polygons:TPoly[], point: TPoint): TPoly | undefined {
    for(const polygon of polygons) {
        if (booleanPointInPolygon(point, polygon)){
            return polygon;
        }
    }

    return undefined;
}

export function roundCoordinates<TG extends Geometry>(g: TG, precision: number): TG {
    const geometry = {...g};
    if (geometry.type === 'Point') {
        geometry.coordinates = roundPosition(geometry.coordinates, precision);
    } else if (geometry.type === 'LineString' || geometry.type === 'MultiPoint') {
        geometry.coordinates = geometry.coordinates.map(coord => roundPosition(coord, precision));
    } else if (geometry.type === 'Polygon' || geometry.type === 'MultiLineString') {
        geometry.coordinates = geometry.coordinates.map(ring => ring.map(coord => roundPosition(coord, precision)));
    } else if (geometry.type === 'MultiPolygon') {
        geometry.coordinates = geometry.coordinates.map(polygon => polygon.map(ring => ring.map(coord => roundPosition(coord, precision))));
    }
    
    return geometry;
}

export function roundPosition(position: Position, precision: number): Position {
    const [a, b] = position;
    return [parseFloat(a.toFixed(precision)), parseFloat(b.toFixed(precision))];
}

/* typeguards */
export { isLatLngLiteralOrLatLng, isLatLngTuple, isLatLangArray, isArrayOfLatLangArray, isPolygonLayer, isPolylineLayer, isMarkerLayer, isKnownShapeLayer};

/* leaflet helper */
export { getBounds, getLayerLeafletId, setLayerLeafletId, getGeoJsonFeatureGeometryFrom };

/* mappings */
export { mapLeafletExpressionToGeoJsonPolygon, mapGeoJsonPolygonToLeafletExpression, mapGeoJsonLineStringToLeafletExpression };

export { findPolygonContainingPoint };