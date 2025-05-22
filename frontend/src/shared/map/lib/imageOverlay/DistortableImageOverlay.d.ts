 import * as L from 'leaflet';

declare module 'leaflet' {

    enum MODES {
        Drag = 'drag',
        Scale = 'scale',
        Distort = 'distort',
        Rotate = 'rotate',
        FreeRotate = 'freeRotate',
        Lock = 'lock'
    }

    // DistortableImageOverlay options interface
    export interface DistortableImageOverlayOptions extends L.ImageOverlayOptions {
        height?: number;
        edgeMinWidth?: number;
        editable?: boolean;
        selected?: boolean;
        mode?: 'distort' | 'rotate' | 'scale' | 'lock' | 'freeRotate';
        actions?: Action[];
        tooltipText?: string;
        enableTooltip?: boolean;
        corners?: L.LatLng[];
        suppressToolbar?: boolean;
        rotation?: {deg: number, rad: number};
        transparent?: boolean;
        translation?: Record<string, string>;
    }

    class Action {}

    class DragAction extends Action {}

    class ScaleAction extends Action {}

    class LockAction extends Action {}

    class RotateAction extends Action {}

    class DeleteAction extends Action{}

    class MagicToolAction extends Action{}

    // DistortableImageOverlay Types + Factory function
    class DistortableImageOverlay extends L.ImageOverlay {
        //#region Props
        options: DistortableImageOverlayOptions;
        edgeMinWidth: number;
        editable: boolean;
        _selected: boolean;
        _url: string;
        rotation: Record<string, any>;
        interactive: boolean;
        tooltipText: string;
        _corners: L.LatLng[];
        _initialDimensions: {
          center: L.Point;
          offset: L.Point;
          zoom: number;
        };
        eP: { editable?: boolean; anyCollected?: () => boolean;} | null;
        editing: {
            enable: () => void;
            disable: () => void;
            _addToolbar: () => void;
            _removeToolbar: () => void;
            _showMarkers: () => void;
            _hideMarkers: () => void;
            _updateToolbarPos?: () => void;
            toolbar?: any;
          };
          edited: boolean;
          //#endregion
        
        constructor(url: string, options?: DistortableImageOverlayOptions)
    
        getCorners(): L.LatLng[];
        getCorner(index: number): L.LatLng;
        setCorners(corners: L.LatLng[]): this;
        setCorner(index: number, latlng: L.LatLng): this;
        getLatLngCenter(): L.LatLng;
        rotateBy(angle: number, unit?: 'deg' | 'rad'): this;
        getAngle(unit?: 'deg' | 'rad'): number;
        setAngle(angle: number, unit?: 'deg' | 'rad'): this;
        select(): void;
        deselect(): void;
        isSelected(): boolean;
        dragBy(formerPoint: L.LatLng, newPoint: L.LatLng): void;
        scaleBy(scale: number): this;
        restore(): this;
        getLatLngCenter(): L.LatLng;
        activateTooltip(): void;
        deactivateTooltip(): void;
        closeToolTip(): void;
        getTooltipText(): string;
        setCorners(latlngObj: Record<string, L.LatLng>): this;
        setCornersFromPoints(pointsObj: Record<string, L.Point>): this;
    }
    
    // Factory function
    function distortableImageOverlay(url: string, options?: DistortableImageOverlayOptions): DistortableImageOverlay;

    namespace DistortableImage {
        class PopupBar {
            
        }
    }
    
    //TrigUtils
    namespace TrigUtil {
        function calcAngle(x: number, y: number, unit?: 'deg' | 'rad'): number;
        function radiansToDegrees(angle: number): number;
        function degreesToRadians(angle: number): number;
    }

    //Utils
    namespace Utils {
        function initTranslation(this: { options: { translation?: Record<string, string> } }): void;

        function getNestedVal<T = any>(
        obj: Record<string, any> | undefined,
        key: string,
        nestedKey: string
        ): T | undefined;
    }

    //DomUtils
    namespace DomUtil {
        let translation: { [key: string]: string };
        function initTranslation(obj: { [key: string]: string }): void;
        function getMatrixString(m: number[]): string;
        function toggleClass(el: HTMLElement, className: string): void;
        function confirmDelete(): boolean;
        function confirmDeletes(n: number): boolean;

        function addClass(el: HTMLElement | SVGSVGElement, name: string): void;
    }

    //MatrixUtil
    namespace MatrixUtil {
        /**
         * Returns the adjugate of a 3x3 Matrix
         * @param m 3x3 Matrix 
         * @returns The adjugate of the given matrix
        */
        function adj(m: number[]): number[];

        /**
         * Multiplies a square 3x3 matrix with another 3x3 matrix
         * @param a 3x3  Matrix 
         * @param b 3x3 Matrix
         */
        function multmm(a: number[], b: number[]): number[];

        /**
         * Multiplies a 3x3 Matrix by a Vector with z fixed to 1
         * @param m 3x3 Matrix
         * @param v Vecotr3(x, y, z = 1 )
         * @returns Vector3
         */
        function multmv(m: number[], v: number[]): number[];

        /**
         * Multiplies a matrix by a scalar
         * @param s Scalar
         * @param m 3x3 Matrix 
         * @returns 3x3 Matrix
         */
        function multsm(s: number, m: number[]): number[];

        /**
         * Computes a projective transformation matrix that maps
         * three basis points (x1,y1), (x2,y2), (x3,y3) to a new
         * coordinate system that includes a fourth point (x4,y4).
         *
         * The result is a 3x3 matrix suitable for projective transformations
         * (e.g., mapping triangles, perspective correction).
         *
         * @param {number} x1 - X-coordinate of the first basis point.
         * @param {number} y1 - Y-coordinate of the first basis point.
         * @param {number} x2 - X-coordinate of the second basis point.
         * @param {number} y2 - Y-coordinate of the second basis point.
         * @param {number} x3 - X-coordinate of the third basis point.
         * @param {number} y3 - Y-coordinate of the third basis point.
         * @param {number} x4 - X-coordinate of the fourth point to map.
         * @param {number} y4 - Y-coordinate of the fourth point to map.
         * @returns {number[]} A 3x3 transformation matrix represented as a flat array of 9 numbers.
         */
        function basisToPoints(
            x1: number, y1: number, x2: number, y2: number,
            x3: number, y3: number, x4: number, y4: number
        ): number[];

        /**
         * Projects a matrix to a normalized X and Y coordinate.
         * @param m 3x3 Matrix
         * @param x Vector3 x
         * @param y Vector3 y
         * @returns An array of 2 points [X, Y]
         */
        function project(m: number[], x: number, y: number): number[];

        /**
         * Computes the 2D projective transformation (homography) matrix
         * that maps four source points to four corresponding destination points.
         *
         * This function is useful for calculating transformations like
         * perspective warping, texture mapping, or simulating camera projection effects.
         *
         * Internally, it finds the basis matrices for the source and destination points,
         * then combines them into a single transformation matrix.
         *
         * @param {number} x1s - X-coordinate of the first source point.
         * @param {number} y1s - Y-coordinate of the first source point.
         * @param {number} x1d - X-coordinate of the first destination point.
         * @param {number} y1d - Y-coordinate of the first destination point.
         * @param {number} x2s - X-coordinate of the second source point.
         * @param {number} y2s - Y-coordinate of the second source point.
         * @param {number} x2d - X-coordinate of the second destination point.
         * @param {number} y2d - Y-coordinate of the second destination point.
         * @param {number} x3s - X-coordinate of the third source point.
         * @param {number} y3s - Y-coordinate of the third source point.
         * @param {number} x3d - X-coordinate of the third destination point.
         * @param {number} y3d - Y-coordinate of the third destination point.
         * @param {number} x4s - X-coordinate of the fourth source point.
         * @param {number} y4s - Y-coordinate of the fourth source point.
         * @param {number} x4d - X-coordinate of the fourth destination point.
         * @param {number} y4d - Y-coordinate of the fourth destination point.
         * @returns {number[]} A normalized 3x3 projective transformation matrix as a flat array of 9 numbers.
         *
         * @see http://franklinta.com/2014/09/08/computing-css-matrix3d-transforms/ for more context.
         */
        function general2DProjection(
            x1s: number, y1s: number, x1d: number, y1d: number,
            x2s: number, y2s: number, x2d: number, y2d: number,
            x3s: number, y3s: number, x3d: number, y3d: number,
            x4s: number, y4s: number, x4d: number, y4d: number
          ): number[];
    }

    //ImageUtil
    namespace ImageUtil {
        /**
         * Calculates the number of centimeters per pixel for a given image overlay.
         * 
         * @param overlay - The image overlay element.
         * @returns Centimeters per pixel.
         */
        function getCmPerPixel(overlay)
    }

    //IconUtil
    namespace IconUtil {
        /**
         * Creates an inline SVG element as a string referencing a given symbol ID.
         * 
         * @param ref - The symbol reference ID (with or without a '#' prefix).
         * @returns A string of SVG markup.
        */
        function create(ref: string): string;

        /**
         * Adds a CSS class to the first SVG element inside the given container.
         * 
         * @param container - The container element.
         * @param loader - The CSS class to add.
         */
        function addClassToSvg(container: HTMLElement, loader: string): void;

        /**
         * Toggles the `xlink:href` attribute between two references inside the first SVG `<use>` element.
         * 
         * @param container - The container element.
         * @param ref1 - The first reference ID.
         * @param ref2 - The second reference ID.
         * @returns The reference currently active after toggle, or `false` if no toggle was done.
         */
        function toggleXlink(container: HTMLElement, ref1: string, ref2: string): string | false;

        /**
         * Toggles the `title` (and `aria-label`) attributes between two strings.
         * 
         * @param container - The container element.
         * @param title1 - The first title option.
         * @param title2 - The second title option.
         * @returns The title currently active after toggle.
         */
        function toggleTitle(container: HTMLElement, title1: string, title2: string): string;
    }


    //Leaflet Augmentation
    interface Map {
        doubleClickLabels?: {
            enabled: () => boolean;
          };
        _latLngToNewLayerPoint(latlng: L.LatLng, zoom: number, center: L.LatLng): L.Point;
    }


    interface Layer {
        _eventParents?: { [key: string]: L.Layer };
    }
}