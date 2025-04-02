import React, { useEffect, useRef, Ref } from "react";
import { Polygon as ReactLeafletPolygon, useMap } from "react-leaflet";
import { LatLngExpression, LatLngLiteral, Polygon as LeafletPolygon, PM } from "leaflet";
import { Polygon as GeoJsonPolygon } from "geojson";
import { mapGeoJsonPolygonToLeafletExpression } from "./utils";

interface BuildingBasePolygonProps {
    editable?: boolean;
    positions: LatLngExpression[][];
    ref?:  Ref<LeafletPolygon<any> | null>;
    onChange?(positions: LatLngLiteral[][]): void;
}

export default function BuildingBasePolygon({positions, ref, editable=true, onChange}: BuildingBasePolygonProps) {
    const map = useMap();
    const polyRef = useRef<LeafletPolygon | null>(null);

    const setRefFunc: React.Ref<LeafletPolygon> = (r) => {
            polyRef.current = r;
            if (ref) {
                if (typeof ref === 'function'){
                    ref(r);
                } else {
                    ref.current = r
                }   
            }
    }

    /* since react-leaflet.Polygon is just wrapper and it never unmounts, 
       can not process ref clean up on remove in ref. That`s why subscribing on leaflet object event.
    */ 
    useEffect(() => {
        const removeHandler = () => setRefFunc(null);
        let pmEditHandler: PM.EditEventHandler | undefined;

        const poly = polyRef.current;
        poly?.on("remove", removeHandler);

        if (onChange) {
            pmEditHandler = function(e) {
                const { geometry } = e.layer.toGeoJSON<GeoJsonPolygon, any>();
                const latLngExpr = mapGeoJsonPolygonToLeafletExpression(geometry);
                onChange(latLngExpr);
            };

            poly?.on("pm:edit", pmEditHandler);
        }

        return () => {
            poly?.off("remove", removeHandler);
            if (pmEditHandler) {
                poly?.off("pm:edit", pmEditHandler);
            }
        };
    }, [polyRef.current]);

    useEffect(() => {
        const eventName = "pm:globaleditmodetoggled";
        const handler: PM.GlobalEditModeToggledEventHandler = ({enabled}) => {
            const poly = polyRef.current;
            if (poly) {
                if (!enabled || !editable) {
                    poly.pm.disable();
                }                
            }
        };
        map.on(eventName, handler);

        return () =>  { map.off(eventName, handler) };
    }, [editable]);

    return <ReactLeafletPolygon
        ref={setRefFunc}
        positions={positions} />
}