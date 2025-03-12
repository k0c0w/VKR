import { MapContainerProps } from "react-leaflet";
import { LatLngBounds } from "leaflet";
import Map, { MapBoundariesSelector } from "../../../features/map";
import { useCallback, useState } from "react";

interface SelectMapBoundariesWidgetProps extends MapContainerProps {
    initialBounds: LatLngBounds;
}

export function SelectMapBoundariesWidget({ initialBounds, zoom=17, ...other }:SelectMapBoundariesWidgetProps) {
    const [_, setBounds] = useState(initialBounds);
    const updateBounds = useCallback((newBounds: LatLngBounds) => setBounds(newBounds), []);

    return <Map
            {...other}
            id="select-boundaries-map"
            zoom={zoom}
            minZoom={1}
            maxZoom={18}
            style={{height: "100%", width: "100%"}}
            attributionControl={false}
        >
        <MapBoundariesSelector initialBounds={initialBounds} setBounds={updateBounds}/>
    </Map>
}