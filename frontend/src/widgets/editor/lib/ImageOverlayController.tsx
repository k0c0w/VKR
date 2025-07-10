import L from "leaflet";
import "leaflet-toolbar";
import "leaflet-distortableimage";
import "@shared/map/lib/imageOverlay/Patch.DistortableImage.Edit";
import "@shared/map/lib/imageOverlay/MagicToolAction";
import "leaflet-toolbar/dist/leaflet.toolbar.css";
import "leaflet-distortableimage/dist/leaflet.distortableimage.css";
import "@shared/map/lib/imageOverlay/style.css";
import { useEffect } from "react";
import { imageOverlayControl as createControl } from "@shared/map/lib/imageOverlay/ImageOverlayControl";
import { useMap } from "react-leaflet";

interface ImageOverlayControllerProps {
  onImageComputeClicked: (imageOverlay: L.DistortableImageOverlay) => void;
}

export default function ImageOverlayController({ onImageComputeClicked}: ImageOverlayControllerProps) {
  const map = useMap();

  useEffect(() => {
    const propagateImage = () => {
      const layer = map.imageOverlayControl?.getOverlayLayer();
      if (layer) {
        onImageComputeClicked(layer);
      }
    }
    map.on("distortableimage:magictoolclicked", propagateImage);

    return () => {
      map?.off("distortableimage:magictoolclicked", propagateImage);
    }
  }, [map, onImageComputeClicked]);

  useEffect(() => {
    const control = createControl({
      position: "bottomleft",
    });

    control.on("imageOverlayControl:overlayChanged", () => {
      const newLayer = control.getOverlayLayer();
      if (newLayer) {
        newLayer.on("select", () => control.showOpacitySlider(true));
        newLayer.on("deselect", () => control.showOpacitySlider(false));
        newLayer.on("remove", () => control.reset())
      }
    });

    control.on("imageOverlayControl:click", () => {
      control.getOverlayLayer()?.select();
    })

    const handleDrawModeToggle = ({enabled}: {enabled: boolean;}) => {
      if (enabled) {
        control?.getOverlayLayer()?.deselect();
        control?.getOverlayLayer()?.editing.disable();
      } else {
        control?.getOverlayLayer()?.editing.enable();
      }
    }

    map.on("pm:globaldrawmodetoggled", handleDrawModeToggle);

    map.addControl(control);

    return () => {
      control.reset();
      map?.removeControl(control);
      map?.off("pm:globaldrawmodetoggled", handleDrawModeToggle);
    };
  }, [map]);

  return <></>;
};
