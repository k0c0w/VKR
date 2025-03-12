import * as geojson from 'geojson';
import * as L from 'leaflet';
declare module 'leaflet' {

  function polygon(latlngs: LatLngExpression[] | LatLngExpression[][], options?: PathTransformPolylineOptions): Polygon;

  interface PathTransformPolylineOptions extends PolylineOptions {
    transform?: boolean;
    draggable?: boolean;
  }

  interface PathTransformOptions {
    handlerOptions?: L.PathOptions;
    boundsOptions?: L.PolylineOptions;
    rotateHandleOptions?: L.PolylineOptions;
    handleLength?: number;
    rotation?: boolean;
    scaling?: boolean;
    uniformScaling?: boolean;
  }

  interface Polygon {
    transform: PathTransform;
    dragging: PathDrag;
  }

  interface PathDrag {
    enable();
    disable();
  }

  interface TransformMatrix {
    transform(coordinates: Point): PointExpression;
  }

  interface PathTransform {
    _matrix: TransformMatrix;
    enable(options?: PathTransformOptions);
    setOptions(options: PathTransformOptions);
  }
}