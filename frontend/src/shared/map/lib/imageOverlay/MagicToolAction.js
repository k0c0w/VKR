import * as L from 'leaflet';
import 'leaflet-distortableimage';

L.MagicToolAction = L.EditAction.extend({
  initialize(map, overlay, options) {
    options = options || {};
    options.toolbarIcon = {
      html: `<svg width="18" height="18" viewBox="0 0 16 16" style="vertical-align: middle;">
               <!-- Wand stick -->
               <path fill="currentColor" d="M3 10 L10 3" stroke="currentColor" stroke-width="1.5"/>
               <!-- Star at tip -->
               <path fill="currentColor" d="M10 3 L10.5 1.5 L12 3 L10.5 4.5 Z"/>
               <!-- Sparkles -->
               <circle fill="currentColor" cx="12" cy="1" r="0.8"/>
               <circle fill="currentColor" cx="13" cy="3" r="0.6"/>
             </svg>`,
      className: 'leaflet-compute-icon',
      tooltip: 'Автоматическая разметка',
    };

    L.EditAction.prototype.initialize.call(this, map, overlay, options);
  },

  addHooks() {
    const edit = this._overlay.editing;
    if (edit instanceof L.DistortableImage.Edit && edit.hasMode('lock') && !edit.isMode('lock')) {
      edit._lock();
    }
    this._map.fireEvent('distortableimage:magictoolclicked');
  },
});