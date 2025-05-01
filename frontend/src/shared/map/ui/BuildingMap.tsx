import 'leaflet/dist/leaflet.css';
import { MapContainer, MapContainerProps } from "react-leaflet";
import { CRS, LatLngBounds } from 'leaflet';
import LevelPickControl from './LevelPickControl';
import BringMapToHomeControl from './BringMapToHomeControl';

interface MapProps extends MapContainerProps {
    disableGeoman?: boolean;
    levelLabels: string[];
    initialLevelIndex: number;
}

const BuildingMap = ({
    levelLabels,
    initialLevelIndex,
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
        minZoom={minZoom}
        maxZoom={maxZoom}
        center={center}
        zoom={zoom}
        style={{height: "100%", width: "100%"}}
        attributionControl={false}
        //crs={CRS.EPSG4326}
        crs={CRS.Simple}
    >
        <LevelPickControl levelLabels={levelLabels} initialSelectedLevelIndex={initialLevelIndex} disableLevelRemoval={levelLabels.length <= 1}/>
        <BringMapToHomeControl home={center} />
        {children}
    </MapContainer>

export default BuildingMap;
