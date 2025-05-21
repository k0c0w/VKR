import { LatLngBounds } from "leaflet";
import { useEffect } from "react";
import { useMap } from "react-leaflet";

export default function FocusOnce({bounds}: {bounds: LatLngBounds}) {
    const map = useMap();

    useEffect(() => {
        map.whenReady(() => map.fitBounds(bounds));
    }, [map]);

    return <></>
}