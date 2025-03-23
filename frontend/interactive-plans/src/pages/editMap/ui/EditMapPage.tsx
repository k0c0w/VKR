import { LatLng } from "leaflet";
import React from "react";
import { EditableMapWidget } from "../../../widgets/map/ui/EditableMapWidget";

const center = new LatLng(55.792690, 49.122388);

export function EditMapPage() {
    return <div style={{width: 800, height: 600}}>
        <EditableMapWidget/>
    </div>
}