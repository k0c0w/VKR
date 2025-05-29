import * as L from 'leaflet';

interface ImageOverlayControlOptions {
  position?: string;
  onImageLoad: (url: string) => void;
}

class ImageOverlayControl extends L.Control {
  showOpacitySlider: (show: boolean) => void;
  reset: () => void;
  on(type: "imageOverlayControl:opacityChange", fn: (e: { opacity: number }) => void): this;
  off(type: "imageOverlayControl:opacityChange", fn: (e: { opacity: number }) => void): this;

  on(type: "imageOverlayControl:click", fn: () => void): this;
  off(type: "imageOverlayControl:click", fn: () => void): this;
}

declare const imageOverlayControl: (options: ImageOverlayControlOptions) => ImageOverlayControl;