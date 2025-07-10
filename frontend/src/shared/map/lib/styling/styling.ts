import { isRoom, isWall, Room, RoomType, Wall } from "@entities/map";
import { Layer, PathOptions } from "leaflet";
import { isPolygonLayer, isPolylineLayer } from "../leafletUtilsAdditions";

export const standartStokeStyle = {
  color: '#666666',
  weight: 2,
};

export const basementStyle = {
  color: '#666666',
  weight: 3,
  fillColor: '#e1e0e5',
  fillOpacity: 0.9,
};

export const hallStyle = {
  ...standartStokeStyle,
  fillColor: '#ffffff',
  fillOpacity: 0.7,
};

export const audienceStyle = {
  ...standartStokeStyle,
  fillColor: '#e2efff',
  fillOpacity: 1,
};

export const wallStyle = {
  ...standartStokeStyle
};

export const errorPolygonStyle = {
  color: '#ff0000',
  weight: 3,
  fillColor: '#ffe6e6',
  fillOpacity: 0.7,
};

export const hightLightStokeStyle = {
  color: '#00FF00',
  weight: 4,
};

export const errorLineStyle = {
  color: '#ff0000',
  weight: 3,
  dashArray: '5 5',
};

export const errorStyle = {
  color: '#ff0000',
  weight: 3,
  fillColor: '#ffe6e6',
  fillOpacity: 0.7,
};

const errorIcon = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" fill="none" stroke="red" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="14" y="36" width="36" height="12" rx="2" fill="none" /><circle cx="20" cy="42" r="1" /><circle cx="26" cy="42" r="1" /><circle cx="32" cy="42" r="1" /><line x1="20" y1="36" x2="16" y2="24" /><line x1="44" y1="36" x2="48" y2="24" /><path d="M12 20 C16 16, 24 16, 28 20" /><path d="M36 20 C40 16, 48 16, 52 20" /></svg>';
const icon = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" fill="none" stroke="#0f9fbc" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="14" y="36" width="36" height="12" rx="2" fill="none" /><circle cx="20" cy="42" r="1" /><circle cx="26" cy="42" r="1" /><circle cx="32" cy="42" r="1" /><line x1="20" y1="36" x2="16" y2="24" /><line x1="44" y1="36" x2="48" y2="24" /><path d="M12 20 C16 16, 24 16, 28 20" /><path d="M36 20 C40 16, 48 16, 52 20" /></svg>';
const highlightIcon = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" fill="none" stroke="#00FF00" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><rect x="14" y="36" width="36" height="12" rx="2" fill="none" /><circle cx="20" cy="42" r="1" /><circle cx="26" cy="42" r="1" /><circle cx="32" cy="42" r="1" /><line x1="20" y1="36" x2="16" y2="24" /><line x1="44" y1="36" x2="48" y2="24" /><path d="M12 20 C16 16, 24 16, 28 20" /><path d="M36 20 C40 16, 48 16, 52 20" /></svg>';

export const itInfrastructureIcon = L.divIcon({
  html: `<div>${icon}</div>`,
  className: "",
  iconSize: [32, 32],
  iconAnchor: [16, 16],
});

export const errorItInfrastructureIcon = L.divIcon({
  html: `<div>${errorIcon}</div>`,
  className: "",
  iconSize: [32, 32],
  iconAnchor: [16, 16],
});

export const highlightItInfrastructureIcon = L.divIcon({
  html: `<div>${highlightIcon}</div>`,
  className: "",
  iconSize: [32, 32],
  iconAnchor: [16, 16],
});

export function getStyleByRoomType(type: RoomType): PathOptions {
  if (type === RoomType.Hall) {
      return hallStyle;
  }
  return audienceStyle;
}

export const minZoomForLabels = 21;

export function getRoomLabel(text: string, mapZoom: number) {
  const fontSize = mapZoom >= minZoomForLabels ? 12 + (mapZoom - minZoomForLabels) * 1.22 : 12;

  return L.divIcon({
                html: `<span style="font-weight: bold; font-size: ${fontSize}px; transform: translateY(-20px);">${text}</span>`,
                className: "",
                iconSize: [0, 0], // Auto-size based on content
                iconAnchor: [0, 0], // Center the icon
              });
}

export function setDefaultStyle(layer: Layer) {
    if (isPolygonLayer(layer)) {
        layer.setStyle(audienceStyle);
    } else if (isPolylineLayer(layer)) {
        layer.setStyle(wallStyle);
    }
}

export function resetStyle(layer: Layer, originalFeature: Wall | Room ) {
    if(isPolylineLayer(layer) && isWall(originalFeature)) {
        layer.setStyle(wallStyle);
    } else if(isPolygonLayer(layer) && isRoom(originalFeature)) {
        layer.setStyle(originalFeature.properties.type === RoomType.Audience ? audienceStyle : hallStyle);   
    }
}