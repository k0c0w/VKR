import { useEffect } from "react";
import { useMap } from "react-leaflet";
import * as protomapsL from "protomaps-leaflet";
import { TilesConfiguration } from "../../../shared/config/mapConfig";

export function PMTileLoader () {
    const map = useMap();

    useEffect(() => {
        const layer = protomapsL.leafletLayer({url: TilesConfiguration.url, theme: TilesConfiguration.theme})
        layer.addTo(map);
    }, [map]);

    return null;
}