import "@geoman-io/leaflet-geoman-free";
import "@geoman-io/leaflet-geoman-free/dist/leaflet-geoman.css";
import { ControlPosition } from "leaflet";
import { useMap } from "react-leaflet";
import { ReactNode, useEffect } from "react";

interface GeomanPluginProps {
    showGeomanControls: boolean,
    children?: ReactNode
}

const toolBarPosition: ControlPosition = "topleft";

export default function GeomanPlugin({ showGeomanControls, children }: GeomanPluginProps) {
    const map = useMap();

    useEffect(() => {
        map.pm.addControls({
            position: toolBarPosition,
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
        });

        return map.pm.removeControls;
    }, [map.pm]);

    useEffect(() => {
    }, [map.pm])

    useEffect(() => {
        const currentControlsVisability = map.pm.controlsVisible();

        if (currentControlsVisability !== showGeomanControls){
            map.pm.toggleControls();
        }
        
    }, [showGeomanControls, map.pm])

    return <>{children}</>;
}
