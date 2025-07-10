import * as L from 'leaflet';
import 'leaflet-distortableimage';

L.EditHandle.prototype._unbindListeners = function () {
    if (this._map) {
        this._map.off('zoomend', this._update, this);
        this._map.off('click', this._toggleRotateScale, this);
        this._map.off('mousedown', this._onHandleDragStart, this);
        this._map.off('mousemove', this._onHandleDrag, this);
        this._map.off('mouseup', this._onHandleDragEnd, this);
        this._map.off('mouseout', this._onHandleDragEnd, this);
    }
};