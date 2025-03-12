import { MapContainerProps } from "react-leaflet";
import Map, { GeomanPlugin }  from "../../../features/map";
import { useState } from "react";


export function EditableMapWidget ({center=[55.792690, 49.122388], zoom=19, ...other}: MapContainerProps) {
    const [mapState] = useState({
        showGeomanControls: true
    });
    const [state, setState] = useState(false);

    return <Map
            {...other}
            center={center}
            zoom={zoom}
            style={{height: "100%", width: "100%"}}
            attributionControl={false}
        >
            <GeomanPlugin showGeomanControls={state}/>
    </Map>
}