import { LatLng } from "leaflet";
import { SelectMapBoundariesWidget } from "../../../widgets/map/ui/SelectMapBoundariesWidget";

const center = new LatLng(55.792690, 49.122388);
const initialBounds = center.toBounds(150);

export function EditMapPage() {
    return <div style={{width: 800, height: 600}}>
        <SelectMapBoundariesWidget center={center} initialBounds={initialBounds}/>
    </div>
}