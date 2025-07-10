import 'leaflet/dist/leaflet.css';
import { MapContainer, MapContainerProps } from "react-leaflet";
import LevelPickControl from "./LevelPickControl";
import BringMapToHomeControl from './BringMapToHomeControl';
import { CreateMapPanesComponent } from './BuildingMapPanes';
import { BuildingMapProvider } from './BuildingMapContext';
import { Building } from '@entities/map';
import BuildingStrcutureLayerGroup from './BuildingStructureLayerGroup';
import GeomanPlugin from '@shared/map/lib/geoman/GeomanPlugin';
import BuildingBasementPolygonLayer from './BuildingBasementPolygonLayer';
import ItEquipmentLayerGroup from './ItEquipmentLayerGroup';
import RoomLabelsLayer from './LabelsLayerGroup';

interface MapProps extends MapContainerProps {
    editable?: boolean;
    building: Building;
    initialLevelIndex: number;
}

export const BuildingMap = ({
    building,
    initialLevelIndex,
    editable = false
    , center = [55.792690, 49.122388]
    , zoom = 19
    , children
    , ...other}:MapProps) => {
    return (<MapContainer
        {...other}
        doubleClickZoom={false}
        minZoom={17}
        maxZoom={24}
        center={center}
        zoom={zoom}
        style={{height: "100%", width: "100%"}}
        attributionControl={false}
    >
        <BuildingMapProvider building={building} initialLevelIndex={initialLevelIndex}>
            <GeomanPlugin showGeomanControls={editable} />
            <LevelPickControl disableLevelRemoval={!editable} />
            <BringMapToHomeControl home={center} />
            <CreateMapPanesComponent />

            <BuildingBasementPolygonLayer />
            <BuildingStrcutureLayerGroup />
            <ItEquipmentLayerGroup />
            <RoomLabelsLayer/>

            {children}
        </BuildingMapProvider>
    </MapContainer>)
    }


