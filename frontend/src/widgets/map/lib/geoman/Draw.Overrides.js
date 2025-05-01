import * as L from "leaflet";

// The only working way to override control and have all functionality.
// Extending Line as separate Wall class and adding it as control does not show tooltip when drawing.
function getWallClass() {
    const originalLine = L.PM.Draw.Line;

    const Wall = originalLine.extend({
        _syncHintMarker(e) {
            // move the cursor marker
            this._hintMarker.setLatLng(e.latlng);
            const args = {
                target: new L.Marker(e.latlng, {
                    draggable: false,
                    icon: L.divIcon({ className: 'marker-icon' }),
                }), 
                type: "VertexSyncHintMarker"
            };

            if (!this._vertexValidation(args)) {
                if (!this.isRed) {
                    this.isRed = true;
                    this._hintline.setStyle({
                      color: '#f00000ff',
                    });
                }
                return;
            }
            originalLine.prototype._syncHintMarker.call(this, e);
        },
        _createVertex(e) {
            const args = {
                target: new L.Marker(e.latlng, {
                    draggable: false,
                    icon: L.divIcon({ className: 'marker-icon' }),
                }), 
                type: "VertexCreationAttempt"
            };
            
            if (!this._vertexValidation(args)) {
                return;
            }

            originalLine.prototype._createVertex.call(this, e);
        },
        _onMarkerDragEnd(e) {
          const marker = e.target;
      
          if (!this._vertexValidationDragEnd(marker)) {
            return;
          }
      
          const { indexPath } = L.PM.Utils.findDeepMarkerIndex(this._markers, marker);
      
          // if self intersection is not allowed but this edit caused a self intersection,
          // reset and cancel; do not fire events
          let intersection = !this.options.allowSelfIntersection && this.hasSelfIntersection();
          if (
            intersection &&
            this.options.allowSelfIntersectionEdit &&
            this._markerAllowedToDrag
          ) {
            intersection = false;
          }
      
          const intersectionReset = this._vertexValidation("move", e) 
            &&
            !this.options.allowSelfIntersection && intersection;
      
          this._fireMarkerDragEnd(e, indexPath, intersectionReset);
      
          if (intersectionReset) {
            // reset coordinates
            this._layer.setLatLngs(this._coordsBeforeEdit);
            this._coordsBeforeEdit = null;
      
            // re-enable markers for the new coords
            this._initMarkers();
      
            if (this.options.snappable) {
              this._initSnappableMarkers();
            }
      
            // check for selfintersection again (mainly to reset the style)
            this._handleLayerStyle();
      
            this._fireLayerReset(e, indexPath);
            return;
          }
          if (
            !this.options.allowSelfIntersection &&
            this.options.allowSelfIntersectionEdit
          ) {
            this._handleLayerStyle();
          }
          // fire edit event
          this._fireEdit();
          this._layerEdited = true;
          this._fireChange(this._layer.getLatLngs(), 'Edit');
        },
        _vertexValidation(e) {
            const marker = e.target;
        
            const addVertexValidation = this.options.addVertexValidation;
            if (addVertexValidation && typeof addVertexValidation === 'function') {
                return addVertexValidation({ layer: this._layer, marker, event: e });
            }
        
            return true;
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