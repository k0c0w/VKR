import L, { LatLngExpression, LatLngBounds, LatLng, TransformMatrix } from "leaflet";
import { useMap } from "react-leaflet";
import { useCallback, useEffect, memo } from "react";
import "leaflet-path-transform";

interface MapBoundariesSelectorProps {
    initialBounds: LatLngBounds,
    setBounds?: (bounds: LatLngBounds) => void,
}

function MapBoundariesSelector (
    { initialBounds, setBounds }: MapBoundariesSelectorProps) {
    const map = useMap();
    /* old lat/long -> old layer cordinates -> new layer cordinates -> new lat/long */ 
    const getTransformedLatLng = useCallback((prevLatLng: LatLng, transformMatrix: TransformMatrix) =>
        map.layerPointToLatLng(transformMatrix.transform(map.latLngToLayerPoint(prevLatLng))), [map]);

    useEffect(() => {
        const initRectBounds: LatLngExpression[] = [
            initialBounds.getNorthWest(),
            initialBounds.getNorthEast(),
            initialBounds.getSouthEast(),
            initialBounds.getSouthWest(),
            initialBounds.getNorthWest()
        ];

        const boundsPolygon = L.polygon(initRectBounds, { transform: true, draggable: true, opacity: 0.5});

        boundsPolygon.once("add", () => {
            boundsPolygon.transform.enable();
            boundsPolygon.dragging.enable();
        });

        if (setBounds) {
            // fired on drag
            boundsPolygon.on("dragend", (e) => {
                const newBoundsLiteral: LatLng[] = e.target.getLatLngs()[0];

                const southWest: LatLng = newBoundsLiteral[3];
                const northEast: LatLng = newBoundsLiteral[1];
    
                setBounds(new LatLngBounds(southWest, northEast));
            });

            // fired on rotate/scale
            boundsPolygon.on("transform", () => {
                const transformMatrix = boundsPolygon.transform._matrix;
                const mapOldCordsToNew = (old: LatLng) => getTransformedLatLng(old, transformMatrix);
    
                const prevCoords = boundsPolygon.getLatLngs()[0] as LatLng[];
                const prevSouthWest: LatLng = prevCoords[3];
                const prevNorthEast: LatLng = prevCoords[1];
                
                const newBounds = new LatLngBounds(
                    mapOldCordsToNew(prevSouthWest),
                    mapOldCordsToNew(prevNorthEast),
                );
                
                setBounds(newBounds);
            });
        }

        boundsPolygon.addTo(map);

        return () => { boundsPolygon.remove(); };
    }, [map, getTransformedLatLng, setBounds, initialBounds])

    return <></>
}

const MapBoundariesSelectorMemorized = memo(MapBoundariesSelector);

export default MapBoundariesSelectorMemorized;