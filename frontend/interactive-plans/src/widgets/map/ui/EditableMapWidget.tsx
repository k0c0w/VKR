import { MapContainerProps } from 'react-leaflet';
import { LatLngExpression, Polygon } from 'leaflet';
import Map from "../ui/Map";
import GeomanPlugin from '../lib/geoman/GeomanPlugin';
import BuildingBasePolygon from '../lib/BuildingBasePolygon';
import { useEffect, useRef, useState } from "react";

export function EditableMapWidget ({center=[0, 0], zoom=7, ...other}: MapContainerProps) {
    const [mapState] = useState({
        showGeomanControls: true
    });

    const [enable] = useState(true);
    const [positions] = useState<LatLngExpression[]>([[0,0], [0,2],[2,2],[2,0],[0,0]])
    useEffect(() => {
        console.log(enable)
    }, [enable])

    const ref = useRef<Polygon | null>(null);

    return <Map
            {...other}
            center={center}
            zoom={-1    }
            style={{height: "100%", width: "100%"}}
            attributionControl={false}
        >  
            <GeomanPlugin showGeomanControls={mapState.showGeomanControls}/>
            <BuildingBasePolygon ref={ref} positions={positions} editable={enable} />
    </Map>
}