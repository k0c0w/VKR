import L from "leaflet";

export const imageOverlayControl = (options) => {
  const photoSvgHtml = `
        <svg class="image-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="18" height="14" rx="2"></rect>
          <circle cx="12" cy="8" r="2"></circle>
          <path d="M3 17l4-4a4 4 0 0 1 6 0l4 4"></path>
        </svg>
      `;

  const ImageOverlayControl = L.Control.extend({
    includes: L.Mixin.Events,

    options: {
      position: "topright",
    },

    initialize: function (opts) {
      L.Util.setOptions(this, opts);
      this._isSliderVisible = false;
      this._container = undefined;
      this._buttonContainer = undefined;
      this._sliderContainer = undefined;
      this._opacitySlider = undefined;
      this._isFileLoaded = false;
      this._controlButton = undefined;
      this._imageOverlay = undefined;
      this._map = undefined;
    },

    onAdd: function (map) {
      this._map = map;
      map.imageOverlayControl = this;
      const container = L.DomUtil.create("div", "leaflet-control image-control");
      this._container = container;

      // Button container
      const buttonContainer = L.DomUtil.create("div", "image-control-button-container", container);
      this._buttonContainer = buttonContainer;

      // Control button with photo icon
      const button = L.DomUtil.create("button", "image-control-button", buttonContainer);
      this._controlButton = button;
      button.innerHTML = photoSvgHtml;
      button.title = "Загрузить схему этажа";
      button.dataset.loaded = this._isFileLoaded.toString(); // Set initial state

      // File input (hidden)
      const fileInput = L.DomUtil.create("input", "image-control-file", buttonContainer);
      this._fileInput = fileInput;
      fileInput.type = "file";
      fileInput.accept = "image/*";
      fileInput.style.display = "none";

      // Separate slider container
      const sliderContainer = L.DomUtil.create("div", "image-control-slider-container", container);
      this._sliderContainer = sliderContainer;
      sliderContainer.style.display = "none";

      // Opacity slider inside its own container
      const sliderInner = L.DomUtil.create("div", "opacity-slider-container", sliderContainer);
      const opacityLabel = L.DomUtil.create("span", "opacity-icon", sliderInner);
      // Eye icon SVG
      opacityLabel.innerHTML = `
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
          <circle cx="12" cy="12" r="3"></circle>
        </svg>
      `;
      const opacitySlider = L.DomUtil.create("input", "", sliderInner);
      this._opacitySlider = opacitySlider;
      opacitySlider.type = "range";
      opacitySlider.min = "0.1";
      opacitySlider.max = "1";
      opacitySlider.step = "0.1";
      opacitySlider.value = "1";

      // Prevent map interactions
      L.DomEvent.disableClickPropagation(container);

      // Event listeners
      L.DomEvent.on(button, "click", () => {
        if (this._isFileLoaded) {
          this.showOpacitySlider(true);
          this.fire('imageOverlayControl:click');
        } else {
          fileInput.click();
        }
      });

      L.DomEvent.on(fileInput, "change", (event) => {
        const input = event.target;
        if (input.files && input.files[0] && !this._isFileLoaded) {
          const file = input.files[0];
          const reader = new FileReader();
          reader.onload = (e) => {
            if (e.target && typeof e.target.result === "string") {
              this._createImageOverlay(e.target.result);
              this._isFileLoaded = true;
              button.dataset.loaded = "true";
              this.showOpacitySlider(true);
            }
          };
          reader.readAsDataURL(file);
        }
      });

      L.DomEvent.on(opacitySlider, "input", (event) => {
        const slider = event.target;
        const opacity = parseFloat(slider.value)
        if (this._imageOverlay) {
          this._imageOverlay.setOpacity(opacity)
        }
        this.fire("imageOverlayControl:opacityChange", { opacity:opacity });
      });

      return container;
    },

    showOpacitySlider(show) {
      if (this._isSliderVisible !== show) {
        this._sliderContainer.style.display = show ? "flex" : "none";
        this._isSliderVisible = show;
      }
    },
    removeOverlay() {
      if (this._imageOverlay) {
        // Disable editing to prevent handle-related errors
        if (this._imageOverlay.editing) {
          this._imageOverlay.editing.disable();
          // Clean up editing handles
          if (this._imageOverlay.editing.currentHandle) {
            const handleLayers = this._imageOverlay.editing.currentHandle._layers || {};
            Object.values(handleLayers).forEach((layer) => {
              if (layer.remove) {
                layer.remove();
              }
            });
            this._imageOverlay.editing.currentHandle = null;
          }
        }
        // Remove the overlay from the map
        this._imageOverlay = undefined;
      }
    },
    reset() {
      this._opacitySlider.value = "1";
      this._controlButton.innerHTML = photoSvgHtml;
      this._controlButton.dataset.loaded = "false";
      this.showOpacitySlider(false);
      this._isFileLoaded = false;
      this._fileInput.value = '';

      this.removeOverlay();
    },

    getOverlayLayer() {
      return this._imageOverlay;
    },

    _createImageOverlay(imageUrl) {
      if (this._imageOverlay) {
        this._map.removeLayer(this._imageOverlay);
      }

      this.reset();

      const newOverlay = L.distortableImageOverlay(imageUrl, {
        mode: "freeRotate",
        interactive: true,
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

      newOverlay.addTo(this._map);
      newOverlay.bringToBack();

      this._imageOverlay = newOverlay;
      this.fire("imageOverlayControl:overlayChanged", newOverlay);
    }
  });

  return new ImageOverlayControl(options);
};