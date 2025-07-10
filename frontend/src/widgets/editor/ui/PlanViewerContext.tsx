import { createContext, useContext, useEffect, useState } from 'react';
import { FeatureWithId, RoomId } from '@entities/map';
import { useBuildingMap } from '@shared/map';
import { isRoom } from '@entities/map';
import { isITEquipment, ItEquipmentId } from '@entities/map/models/ItEquipment';

type FocusedFeatureId = RoomId | ItEquipmentId;

interface PlanViewerContextType {
  focusedFeature: FeatureWithId<FocusedFeatureId> | null;
  focusOn: (feature: FeatureWithId<FocusedFeatureId>) => void;
  unfocus: () => void;
}

const PlanViewerContext = createContext<PlanViewerContextType | undefined>(undefined);

export function usePlanViewerContext() {
  const context = useContext(PlanViewerContext);
  if (!context) {
    throw new Error('usePlanViewerContext must be used within a PlanViewerContextProvider');
  }
  return context;
}

interface PlanViewerProviderProps {
  children: React.ReactNode;
}

export function PlanViewerContextProvider({ children }: PlanViewerProviderProps) {
  const { highlightRoom, unhighlightRoom, highlightItEquipment, unhighlightItEquipment } = useBuildingMap();
  const [focusedFeature, setFocusedFeature] = useState<FeatureWithId<FocusedFeatureId> | null>(null);

  useEffect(() => {
    const feature = focusedFeature;
    if (feature) {
      if (isRoom(feature)) {
        highlightRoom(feature.id);
      } else if (isITEquipment(feature)) {
        highlightItEquipment(feature.id as ItEquipmentId);
      }
    }

    return () => {
      if (feature) {
        if (isRoom(feature)) {
          unhighlightRoom(feature.id);
        } else if (isITEquipment(feature)) {
          unhighlightItEquipment(feature.id as ItEquipmentId);
        }
      }
    }
  }, [focusedFeature]);

  const focusOn = (feature: FeatureWithId<FocusedFeatureId>) => {
    // Clear previous highlight
    setFocusedFeature(null);
    setFocusedFeature(feature);
  };

  const unfocus = () => {
    if (focusedFeature) {
      if (isRoom(focusedFeature)) {
        unhighlightRoom(focusedFeature.id);
      } else if (isITEquipment(focusedFeature)) {
        unhighlightItEquipment(focusedFeature.id as ItEquipmentId);
      }
    }
    setFocusedFeature(null);
  };

  return (
    <PlanViewerContext.Provider value={{ focusedFeature, focusOn, unfocus }}>
      {children}
    </PlanViewerContext.Provider>
  );
}