import L from 'leaflet';

export interface LevelPickControl extends L.Control {
  changeLevel(levelIndex: number): void;
  setLevels(levelLabels: string[], selectLevelIndex?: number): void;
  setRubbishBinDisabled(disabled: boolean): void;
  setControlDisabled(disabled: boolean): void;
  setShowLevelButtons(show: boolean): void;
  selectedLevelIndex: number;
  levelLabels: string[];
}

declare module 'leaflet' {
  type LevelChangeEventHandler = (e: { levelIndex: number; label: string }) => void;
  type LevelRemoveButtonClickedEventHandler = (e: { levelIndex: number; label: string }) => void;
  type LevelEditButtonClickedEventHandler = (e: { levelIndex: number; label: string }) => void;
  type LevelAddButtonClickedEventHandler = () => void;

  interface Map {
    levelControl: L.Control.LevelPicker | undefined;
  }

  interface Evented {
    on(type: 'levelpicker:changelevel', fn: LevelChangeEventHandler): this;
    once(type: 'levelpicker:changelevel', fn: TypeLevelChangeEventHandler): this;
    off(type: 'levelpicker:changelevel', fn: LevelChangeEventHandler): this;

    on(type: 'levelpicker:removelevelclicked', fn: LevelRemoveButtonClickedEventHandler): this;
    once(type: 'levelpicker:removelevelclicked', fn: LevelRemoveButtonClickedEventHandler): this;
    off(type: 'levelpicker:removelevelclicked', fn: LevelRemoveButtonClickedEventHandler): this;

    on(type: 'levelpicker:editlevelbuttonclicked', fn: LevelEditButtonClickedEventHandler): this;
    once(type: 'levelpicker:editlevelbuttonclicked', fn: LevelEditButtonClickedEventHandler): this;
    off(type: 'levelpicker:editlevelbuttonclicked', fn: LevelEditButtonClickedEventHandler): this;

    on(type: 'levelpicker:addbuttonclicked', fn: LevelAddButtonClickedEventHandler): this;
    once(type: 'levelpicker:addbuttonclicked', fn: LevelAddButtonClickedEventHandler): this;
    off(type: 'levelpicker:addbuttonclicked', fn: LevelAddButtonClickedEventHandler): this;
  }

  namespace Control {
    class LevelPicker extends Control {
      constructor(options?: {
        position?: string;
        levelLabels?: string[];
        selectedLevelIndex?: number;
        disableRubbishBin?: boolean;
        showLevelButtons?: boolean;
        controlDisabled?: boolean;
      });
      changeLevel(levelIndex: number): void;
      setLevels(levelLabels: string[], selectLevelIndex?: number): void;
      setRubbishBinDisabled(disabled: boolean): void;
      setControlDisabled(disabled: boolean): void;
      setShowLevelButtons(show: boolean): void;
      selectedLevelIndex: number;
      levelLabels: string[];
    }
  }

  namespace control {
    function levelPicker(options?: {
      position?: string;
      levelLabels?: string[];
      selectedLevelIndex?: number;
      disableRubbishBin?: boolean;
      showLevelButtons?: boolean;
      controlDisabled?: boolean;
    }): Control.LevelPicker;
  }
}