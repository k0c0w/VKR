import "@geoman-io/leaflet-geoman-free";
import "@geoman-io/leaflet-geoman-free/dist/leaflet-geoman.css";
import { ControlPosition } from "leaflet";
import { useMap } from "react-leaflet";
import { ReactNode, useEffect } from "react";
import { overrideDraw } from "./Draw.Overrides";

interface GeomanPluginProps {
    showGeomanControls: boolean,
    children?: ReactNode
}

const toolBarPosition: ControlPosition = "topleft";

export default function GeomanPlugin({ showGeomanControls, children }: GeomanPluginProps) {
    const map = useMap();

    useEffect(() => {
        map.pm.setLang("ru");
        overrideDraw(map);

        map.pm.addControls({
            position: toolBarPosition,
            cutPolygon: false,
            rotateMode: true,
            drawCircle: false,
            drawCircleMarker: false,
            drawMarker: true,
            drawText: false,
            dragMode: true,
            editControls: true,
            editMode: true,
            drawPolygon: true,
            drawPolyline: true,
            drawRectangle: false,
            removalMode: true,
        });

        return () => {
            map?.pm?.removeControls();
        } 
    }, [map]);

    useEffect(() => {
        const currentControlsVisability = map.pm.controlsVisible();

        if (currentControlsVisability !== showGeomanControls){
            map.pm.toggleControls();
        }
        
    }, [showGeomanControls, map])

    return <>{children}</>;
}
