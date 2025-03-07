import { MapContainer, MapContainerProps } from "react-leaflet";
import { PMTileLoader } from "../lib/PMTileLoader";
import { GeomanPluginInitializer } from "../lib/GeomanPluginInitializer";
import 'leaflet/dist/leaflet.css';

export function EditableMapWidget ({center=[55.792690, 49.122388], zoom=19, ...other}:MapContainerProps) {

    return <MapContainer
            {...other}
            center={center}
            zoom={zoom}
            style={{height: "100%", width: "100%"}}
            attributionControl={false}
        >
        <PMTileLoader/>
        <GeomanPluginInitializer/>
    </MapContainer>
}