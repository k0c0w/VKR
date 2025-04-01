import 'leaflet/dist/leaflet.css';
import { MapContainer, MapContainerProps } from "react-leaflet";
import { CRS } from 'leaflet';
import GeomanPlugin from '../lib/geoman/GeomanPlugin';

interface MapProps extends MapContainerProps {
    disableGeoman?: boolean;
}

const Map = ({
    disableGeoman = false
    , center = [55.792690, 49.122388]
    , zoom = 19
    , minZoom = 0
    , maxZoom = 22
    , children
    , ...other}:MapProps) => 
    <MapContainer
        doubleClickZoom={false}
        {...other}
        crs={CRS.Simple}
        minZoom={minZoom}
        maxZoom={maxZoom}
        center={center}
        zoom={zoom}
        style={{height: "100%", width: "100%"}}
        attributionControl={false}
    >
        <GeomanPlugin showGeomanControls={!disableGeoman} />
        {children}
    </MapContainer>

export default Map;
