import * as L from 'leaflet';

interface ImageOverlayControlOptions {
  position?: string;
}

class ImageOverlayControl extends L.Control {
  showOpacitySlider: (show: boolean) => void;
  removeOverlay();
  reset();
  getOverlayLayer: () => L.DistortableImageOverlay | undefined;
  on(type: "imageOverlayControl:opacityChange", fn: (e: { opacity: number }) => void): this;
  off(type: "imageOverlayControl:opacityChange", fn: (e: { opacity: number }) => void): this;

  on(type: "imageOverlayControl:click", fn: () => void): this;
  off(type: "imageOverlayControl:click", fn: () => void): this;

  on(type: "imageOverlayControl:overlayChanged", fn: (overlay: L.DistortableImageOverlay) => void);
  off(type: "imageOverlayControl:overlayChanged", fn: (overlay: L.DistortableImageOverlay) => void);
}

declare const imageOverlayControl: (options: ImageOverlayControlOptions) => ImageOverlayControl;

declare module 'leaflet' {
  interface Map {
    imageOverlayControl: ImageOverlayControl | undefined;
  }
}