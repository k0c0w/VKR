import "leaflet/dist/leaflet.css";
import "@geoman-io/leaflet-geoman-free";
import "@geoman-io/leaflet-geoman-free/dist/leaflet-geoman.css";
import { useEffect } from "react";
import { useMap } from "react-leaflet";

export function GeomanPluginInitializer() {
    const map = useMap();

    useEffect(() => {
        map.pm.addControls({
            position: "topleft",
            cutPolygon: false,
            rotateMode: true,
            drawCircle: false,
            drawCircleMarker: false,
            drawMarker: false,
            drawText: false,
            dragMode: true,
            editControls: true,
            editMode: true,
            drawPolygon: true,
            drawPolyline: true,
            drawRectangle: true,
            removalMode: true,
        })
    })
    return null;
}