import * as L from 'leaflet';
import 'leaflet-distortableimage';

L.DistortableImage.Edit.prototype._removeOverlay = function () {
    const ov = this._overlay;
    const eP = this.parentGroup;

    if (this.isMode('lock') || !this.hasTool(L.DeleteAction)) { return; }

    this._removeToolbar();

    if (eP) { eP.removeLayer(ov); }
    else { ov._map.removeLayer(ov); }
}