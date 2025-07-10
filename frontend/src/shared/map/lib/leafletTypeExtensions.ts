import {Layer} from "leaflet";
import guid from "@shared/types/guid";
export interface LayerWithFeatureId extends Layer {
  featureId: guid | number;
}

export function mutateToLayerWithFeatureIdBasedOn(layer: Layer, featureId: guid | number): LayerWithFeatureId {
  //@ts-ignore
  layer.featureId = featureId;

  return layer as LayerWithFeatureId;
}

export function castToLayerWithFeatureId(layer: Layer): LayerWithFeatureId {
  const l = layer as LayerWithFeatureId;
  if (l.featureId === undefined) {
    throw new Error("Layer does not have property featureId! Can not cast to LayerWithFeatureId.")
  }

  return l;
}

export function layerHasFeatureId(layer: Layer): layer is LayerWithFeatureId {

  // @ts-ignore
  return layer.featureId !== undefined;
}