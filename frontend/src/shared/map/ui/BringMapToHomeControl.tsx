import L, { LatLngExpression } from "leaflet";
import { useEffect } from "react";
import 'leaflet-easybutton';
import 'leaflet-easybutton/src/easy-button.css';
import { useBuildingMap } from "./BuildingMapContext";

const icon = '<svg width="16" height="16" viewBox="0 0 32 32" preserveAspectRatio="none" class="sc-aXZVg duXPgK" style="stroke: rgb(44, 44, 44); fill: rgb(44, 44, 44); pointer-events: none;"><g stroke="none"><path d="M1.25 4C1.11193 4 1 4.11193 1 4.25V27.75C1 27.8881 1.11193 28 1.25 28H1.75C1.88807 28 2 27.8881 2 27.75V4.25C2 4.11193 1.88807 4 1.75 4H1.25Z"></path><path d="M30 4.25C30 4.11193 30.1119 4 30.25 4H30.75C30.8881 4 31 4.11193 31 4.25V27.75C31 27.8881 30.8881 28 30.75 28H30.25C30.1119 28 30 27.8881 30 27.75V4.25Z"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M9 8H23C23.5523 8 24 8.44772 24 9V23C24 23.5523 23.5523 24 23 24H9C8.44772 24 8 23.5523 8 23V9C8 8.44772 8.44772 8 9 8ZM23 9H9V23H23V9Z"></path></g></svg>';
export default function BringMapToHomeControl({home}: {home: LatLngExpression}) {
    const { map } = useBuildingMap();
    useEffect(() => {
        const bar = L.easyBar([
            L.easyButton(icon, function(_, map) {
                map.setView(home);
            },
            "Оцентровать карту")
        ],
        {
            position: "topleft",
        })
        
        bar.addTo(map);

        return () => {
            bar.remove();
        }
    }, [home]);

    return <></>
}