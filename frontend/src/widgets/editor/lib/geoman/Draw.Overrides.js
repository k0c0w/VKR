import * as L from "leaflet";

// The only working way to override control and have all functionality.
// Extending Line as separate Wall class and adding it as control does not show tooltip when drawing.
function getWallClass() {
    const originalLine = L.PM.Draw.Line;

    const Wall = originalLine.extend({
      enable(options) {
          L.Util.setOptions(this, options);
      
          // enable draw mode
          this._enabled = true;
      
          this._markers = [];
      
          // create a new layergroup
          this._layerGroup = new L.FeatureGroup();
          this._layerGroup._pmTempLayer = true;
          this._layerGroup.addTo(this._map);
      
          // this is the polyLine that'll make up the polygon
          this._layer = L.polyline([], {
            ...this.options.templineStyle,
            pmIgnore: false,
          });
          this._setPane(this._layer, 'layerPane');
          this._layer._pmTempLayer = true;
          this._layerGroup.addLayer(this._layer);
        
          // this is the hintline from the mouse cursor to the last marker
          this._hintline = L.polyline([], this.options.hintlineStyle);
          this._setPane(this._hintline, 'layerPane');
          console.log(this._hintline.options.pane);
          this._hintline._pmTempLayer = true;
          this._layerGroup.addLayer(this._hintline);
        
          // this is the hintmarker on the mouse cursor
          this._hintMarker = L.marker(this._map.getCenter(), {
            interactive: false, // always vertex marker below will be triggered from the click event -> _finishShape #911
            zIndexOffset: 100,
            icon: L.divIcon({ className: 'marker-icon cursor-marker' }),
          });
          this._setPane(this._hintMarker, 'vertexPane');
          this._hintMarker._pmTempLayer = true;
          this._layerGroup.addLayer(this._hintMarker);
        
          // show the hintmarker if the option is set
          if (this.options.cursorMarker) {
            L.DomUtil.addClass(this._hintMarker._icon, 'visible');
          }
        
          // add tooltip to hintmarker
          if (this.options.tooltips) {
            this._hintMarker
              .bindTooltip('Начать фигуру', {
                permanent: true,
                offset: L.point(0, 10),
                direction: 'bottom',
              
                opacity: 0.8,
              })
              .openTooltip();
          }
        
          // change map cursor
          this._map.getContainer().classList.add('geoman-draw-cursor');
        
          // create a polygon-point on click
          this._map.on('click', this._createVertex, this);
        
          // finish on layer event
          // #http://leafletjs.com/reference.html#interactive-layer-click
          if (this.options.finishOn && this.options.finishOn !== 'snap') {
            this._map.on(this.options.finishOn, this._finishShape, this);
          }
        
          // prevent zoom on double click if finishOn is === dblclick
          if (this.options.finishOn === 'dblclick') {
            this.tempMapDoubleClickZoomState = this._map.doubleClickZoom._enabled;
          
            if (this.tempMapDoubleClickZoomState) {
              this._map.doubleClickZoom.disable();
            }
          }
        
          // sync hint marker with mouse cursor
          this._map.on('mousemove', this._syncHintMarker, this);
        
          // sync the hintline with hint marker
          this._hintMarker.on('move', this._syncHintLine, this);
        
          // toggle the draw button of the Toolbar in case drawing mode got enabled without the button
          this._map.pm.Toolbar.toggleButton(this.toolbarButtonName, true);
        
          // an array used in the snapping mixin.
          // TODO: think about moving this somewhere else?
          this._otherSnapLayers = [];
        
          // make sure intersection is not set while start drawing
          this.isRed = false;
        
          // fire drawstart event
          this._fireDrawStart();
          this._setGlobalDrawMode();
        },
    });

    return Wall;
}

// The only working way to override control and have all functionality.
// Extending Polygon as separate Room class and adding it as control does not show tooltip when drawing.
function getRoomClass(wallClass) {
    const Room = wallClass.extend({
        initialize(map) {
            this._map = map;
            this._shape = 'Polygon';
            this.toolbarButtonName = 'drawPolygon';
          },
        enable(options) {
            L.PM.Draw.Line.prototype.enable.call(this, options);
            // Overwrite the shape "Line" of this._layer
            this._layer.pm._shape = 'Polygon';
        },
        _createMarker(latlng) {
            // create the new marker
            const marker = new L.Marker(latlng, {
              draggable: false,
              icon: L.divIcon({ className: 'marker-icon' }),
            });
            this._setPane(marker, 'vertexPane');

            // mark this marker as temporary
            marker._pmTempLayer = true;

            // add it to the map
            this._layerGroup.addLayer(marker);
            this._markers.push(marker);

            // if the first marker gets clicked again, finish this shape
            if (this._layer.getLatLngs().flat().length === 1) {
              marker.on('click', this._finishShape, this);

              // add the first vertex to "other snapping layers" so the polygon is easier to finish
              this._tempSnapLayerIndex = this._otherSnapLayers.push(marker) - 1;

              if (this.options.snappable) {
                this._cleanupSnapping();
              }
            } else {
              // add a click event w/ no handler to the marker
              // event won't bubble so prevents creation of identical markers in same polygon
              // fixes issue where double click during poly creation when allowSelfIntersection: false caused it to break
              marker.on('click', () => 1);
            }

            return marker;
        },
        _setTooltipText() {
            const { length } = this._layer.getLatLngs().flat();
            let text = '';

            // handle tooltip text
            if (length <= 2) {
              text = 'Продолжить стену'
            } else {
              text = 'Создать комнату';
            }
            this._hintMarker.setTooltipContent(text);
        },
        _finishShape() {
            // if self intersection is not allowed, do not finish the shape!
            if (!this.options.allowSelfIntersection) {
              // Check if polygon intersects when is completed and the line between the last and the first point is drawn
              this._handleSelfIntersection(true, this._layer.getLatLngs()[0]);

              if (this._doesSelfIntersect) {
                return;
              }
            }

            // If snap finish is required but the last marker wasn't snapped, do not finish the shape!
            if (
              this.options.requireSnapToFinish &&
              !this._hintMarker._snapped &&
              !this._isFirstLayer()
            ) {
              return;
            }

            // get coordinates
            const coords = this._layer.getLatLngs();

            // only finish the shape if there are 3 or more vertices
            if (coords.length <= 2) {
              return;
            }

            const polygonLayer = L.polygon(coords, this.options.pathOptions);
            this._setPane(polygonLayer, 'layerPane');
            this._finishLayer(polygonLayer);
            polygonLayer.addTo(this._map.pm._getContainingLayer());

            // fire the pm:create event and pass shape and layer
            this._fireCreate(polygonLayer);

            // clean up snapping states
            this._cleanupSnapping();

            // remove the first vertex from "other snapping layers"
            this._otherSnapLayers.splice(this._tempSnapLayerIndex, 1);
            delete this._tempSnapLayerIndex;

            const hintMarkerLatLng = this._hintMarker.getLatLng();

            // disable drawing
            this.disable();
            if (this.options.continueDrawing) {
              this.enable();
              this._hintMarker.setLatLng(hintMarkerLatLng);
            }
        },
    });

    return Room;
}



function overrideDraw(map) {
  const Wall = getWallClass();
  map.pm.Draw.Line = new Wall(map);
  
  const Room = getRoomClass(Wall);
  map.pm.Draw.Polygon = new Room(map);
}

export { overrideDraw };