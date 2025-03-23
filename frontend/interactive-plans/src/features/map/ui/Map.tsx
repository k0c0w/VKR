import 'leaflet/dist/leaflet.css';
import { MapContainer, MapContainerProps } from "react-leaflet";
import React from 'react';
import { CRS } from 'leaflet';

const Map = ({
    center = [55.792690, 49.122388]
    , zoom = 19
    , minZoom = 0
    , maxZoom = 22
    , children
    , ...other}:MapContainerProps) => 
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
        {children}
    </MapContainer>

export default Map;
