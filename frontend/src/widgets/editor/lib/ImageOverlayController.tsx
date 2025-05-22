import L from "leaflet";
import "leaflet-toolbar";
import "leaflet-distortableimage";
import "@shared/map/lib/imageOverlay/Patch.DistortableImage.Edit";
import "@shared/map/lib/imageOverlay/MagicToolAction";
import "leaflet-toolbar/dist/leaflet.toolbar.css";
import "leaflet-distortableimage/dist/leaflet.distortableimage.css";
import "@shared/map/lib/imageOverlay/style.css";
import { useState, useEffect } from "react";
import { useMap } from "react-leaflet";
import { ImageOverlayControl, imageOverlayControl as createControl } from "@shared/map/lib/imageOverlay/ImageOverlayControl";

interface ImageOverlayControllerProps {
  readonlyMode: boolean;
  onImageComputeClicked: (imageUrl: string) => void;
}

export default function ImageOverlayController({readonlyMode, onImageComputeClicked}: ImageOverlayControllerProps) {
  const map = useMap();
  const [imageOverlayControl, setImageOverlayControl] = useState<ImageOverlayControl | null>(null);
  const [imageOverlay, setImageOverlay] = useState<L.DistortableImageOverlay | null>(null);
  const [imageUrl, setImageUrl] = useState<string | null>(null);

  useEffect(() => {
    const propagateImage = () => {
      if (imageUrl) {
        onImageComputeClicked(imageUrl)
      }
    }
    map.on("distortableimage:magictoolclicked", propagateImage);

    return () => {
      map.off("distortableimage:magictoolclicked", propagateImage);
    }
  }, [map, imageUrl, onImageComputeClicked]);

  useEffect(() => {
    const changeOpacity = ({opacity}:{opacity: number;}) => {
      if (imageOverlay) {
        imageOverlay.setOpacity(opacity)
      }
    }

    imageOverlayControl?.on("imageOverlayControl:opacityChange", changeOpacity);

    return () => {
          imageOverlayControl?.off("imageOverlayControl:opacityChange", changeOpacity);
    }
  }, [imageOverlay, imageOverlayControl]);

  useEffect(() => {
    if (imageOverlay && imageOverlayControl) {
      imageOverlay.on("select", () => imageOverlayControl.showOpacitySlider(true));
      imageOverlay.on("deselect", () => imageOverlayControl.showOpacitySlider(false));
      imageOverlay.on("remove", () => {
        imageOverlayControl.reset();
        setImageOverlay(null);
        setImageUrl(null);
      });
    }
  }, [imageOverlay, imageOverlayControl]);

  useEffect(() => {
    const disableImageOverlay = () => imageOverlay?.editing.disable();
    const enableImageOverlay = () => imageOverlay?.editing.enable();

    map.on("pm:drawstart", disableImageOverlay);
    map.on("pm:drawend", enableImageOverlay);

    return () => {
      map.off("pm:drawstart", disableImageOverlay);
      map.off("pm:drawend", enableImageOverlay);
    };
  }, [map, imageOverlay]);

  useEffect(() => {
    if (readonlyMode) {
      return;
    }

    const control = createControl({
      position: "bottomleft",
      onImageLoad: (url: string) => {
        if (imageOverlay) {
          map.removeLayer(imageOverlay);
        }
        const newOverlay = L.distortableImageOverlay(url, {
          mode: "freeRotate",
          selected: true,
          actions: [L.DragAction, L.ScaleAction, L.LockAction, L.RotateAction, L.MagicToolAction, L.DeleteAction],
          translation: {
              deleteImage: 'Удалить',
              dragImage: 'Переместить',
              lockMode: 'Зафиксировать',
              rotateImage: 'Повернуть',
              scaleImage: 'Масштабировать',
          }
        });

        newOverlay.setZIndex(200);

        newOverlay.addTo(map);
        setImageOverlay(newOverlay);
        setImageUrl(url);
      }
    });

    map.addControl(control);
    setImageOverlayControl(control);

    return () => {
      if (readonlyMode) {
        return;
      }

      if (imageOverlay) {
        map.removeLayer(imageOverlay);
      }
      map.removeControl(control);
      setImageOverlayControl(null);
    };
  }, [map, readonlyMode]);

  return <></>;
};
