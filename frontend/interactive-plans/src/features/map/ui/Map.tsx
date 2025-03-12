import 'leaflet/dist/leaflet.css';
import { MapContainer, MapContainerProps } from "react-leaflet";
import { PMTileLoader } from "../lib/PMTileLoader";

const Map = ({
    center = [55.792690, 49.122388]
    , zoom = 19
    , minZoom = 0
    , maxZoom = 22
    , children
    , ...other}:MapContainerProps) => 
    <MapContainer
        {...other}
        minZoom={minZoom}
        maxZoom={maxZoom}
        center={center}
        zoom={zoom}
        style={{height: "100%", width: "100%"}}
        attributionControl={false}
    >
        <PMTileLoader/>
        {children}
    </MapContainer>

export default Map;
