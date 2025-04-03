import React, { useEffect, useRef, Ref } from "react";
import { Polygon as ReactLeafletPolygon } from "react-leaflet";
import { LatLngExpression, LatLngLiteral, Polygon as LeafletPolygon, PM } from "leaflet";

function enableLayer(polygon: LeafletPolygon) {
    polygon.pm.enable({
        allowSelfIntersection: false,
        allowSelfIntersectionEdit: false,
        removeLayerBelowMinVertexCount: false,
        allowRemoval: false,
        allowCutting: false,
        allowRotation: false,
        draggable: false,
        snappable: false,

        allowEditing: true,
    })
}

function disableLayer(polygon: LeafletPolygon) {
    polygon.pm.disable();
    polygon.pm.setOptions({
        allowSelfIntersection: false,
        allowSelfIntersectionEdit: false,
        removeLayerBelowMinVertexCount: false,
        allowRemoval: false,
        allowCutting: false,
        allowRotation: false,
        draggable: false,
        allowEditing: false,

        snappable: true,
        snapSegment: true,
        snapMiddle: true,
    })
}

interface BuildingBasePolygonProps {
    editable?: boolean;
    positions: LatLngExpression[][];
    ref?:  Ref<LeafletPolygon<any> | null>;
    onChange?(positions: LatLngLiteral[][]): void;
}

export default function BuildingBasePolygon({positions, ref, editable=true, onChange}: BuildingBasePolygonProps) {
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

    useEffect(() => {
        const polygon = polyRef.current;
        if (polygon) {
            if (editable) {
                enableLayer(polygon);
            } else {
                disableLayer(polygon);
            }
        }
    }, [editable, polyRef]);

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
                if (e.shape !== 'Polygon') {
                    throw Error("Only 'Polygon' is supported for BuildingBase!");
                }

                const polygonLayer = e.layer as LeafletPolygon;
                const latLngs = polygonLayer.getLatLngs() as LatLngLiteral[][];
                onChange(latLngs);
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

    return <ReactLeafletPolygon
        ref={setRefFunc}
        positions={positions} />
}