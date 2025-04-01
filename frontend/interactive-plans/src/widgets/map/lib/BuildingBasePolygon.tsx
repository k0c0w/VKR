import React, { useEffect, useRef, Ref } from "react";
import { Polygon as ReactLeafletPolygon, useMap } from "react-leaflet";
import { LatLngExpression, Polygon as LeafletPolygon, PM } from "leaflet";

interface BuildingBasePolygonProps {
    editable?: boolean;
    positions: LatLngExpression[];
    ref?:  Ref<LeafletPolygon<any> | null>
}

export default function BuildingBasePolygon({positions, ref, editable=true}: BuildingBasePolygonProps) {
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
        const eventName = "remove";
        const handler = () => setRefFunc(null);

        const poly = polyRef.current;
        poly?.on(eventName, handler);

        return () => {
            poly?.off(eventName, handler);
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