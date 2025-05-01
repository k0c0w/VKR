import { getLayerLeafletId } from "@shared/map/lib/leafletUtilsAdditions";
import { Layer } from "leaflet";
import { useState } from "react";

export function useLocalLayersStorage<T extends Layer>(initialState?:{[layerId: number]: T} | undefined): 
    [{[layerId: number]: T}, (layer: T, options?: {
        delete: boolean;
    }) => void] {
    const [layers, setLayers] = useState<{[layerId: number]: T}>(initialState ?? {});
    const setLayer = (layer: T, options?: {delete: boolean}) => setLayers(prev => {
        const updatedLayers = {...prev};
    
        if (options && options.delete) {
            delete updatedLayers[getLayerLeafletId(layer)];
        } else {
            updatedLayers[getLayerLeafletId(layer)] = layer;
        }
    
        return updatedLayers;
    });

    return [
        layers,
        setLayer
    ];
}