import React, { useEffect, useRef, Ref } from "react";
import { Polygon as ReactLeafletPolygon } from "react-leaflet";
import { LatLngExpression, Polygon as LeafletPolygon, PM } from "leaflet";
import { GEOJSON_PRECISION } from "@app/config/constants";
import { Feature, Polygon as GeoJsonPolygon } from "geojson";
import { basementStyle } from "../../../widgets/map/lib/styling/styling";

interface BuildingBasePolygonProps {
    editable: boolean;
    positions: LatLngExpression[][];
    ref?: Ref<LeafletPolygon<any> | null>;
    onChange?(positions: Feature<GeoJsonPolygon>): void;
}

export default function BuildingBasePolygon({
    positions,
    ref,
    editable,
    onChange,
}: BuildingBasePolygonProps) {
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
                polygon.pm.enable({
                    allowSelfIntersection: false,
                    allowSelfIntersectionEdit: false,
                    removeLayerBelowMinVertexCount: false,
                    allowRemoval: false,
                    allowCutting: false,
                    allowRotation: true,
                    draggable: false,
                    snappable: false,
                    allowEditing: true,
                });
            } else {
                polygon.pm.disable();
                polygon.pm.setOptions({
                    draggable: false,
                    allowEditing: false,
                    allowSelfIntersection: false,
                    allowSelfIntersectionEdit: false,
                    removeLayerBelowMinVertexCount: false,
                    allowRemoval: false,
                    allowCutting: false,
                    allowRotation: false,
                    snappable: false,
                });
            }
        }

    }, [editable, polyRef]);

    useEffect(() => {
        const removeHandler = () => setRefFunc(null);
        let pmEditHandler: PM.EditEventHandler | undefined;

        const poly = polyRef.current;
        poly?.on("remove", removeHandler);

        if (onChange && editable) {
            pmEditHandler = function (e) {
                if (e.shape !== "Polygon") {
                    throw new Error("Only 'Polygon' is supported for BuildingBase!");
                }

                const polygonLayer = e.layer as LeafletPolygon;
                const polygon = polygonLayer.toGeoJSON(GEOJSON_PRECISION)
                if (polygon.geometry.type !== "Polygon") {
                    throw new Error("Unsupported polygon shape!");
                }

                onChange(polygon as Feature<GeoJsonPolygon>);
            };

            poly?.on("pm:edit", pmEditHandler);
        }

        return () => {
            poly?.off("remove", removeHandler);
            if (pmEditHandler) {
                poly?.off("pm:edit", pmEditHandler);
            }
        };
    }, [onChange, editable]);

    return <ReactLeafletPolygon ref={setRefFunc} positions={positions} pathOptions={basementStyle} />;
}
