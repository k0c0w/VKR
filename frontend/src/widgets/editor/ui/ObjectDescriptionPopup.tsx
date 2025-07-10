import { ReactNode, useEffect, useRef } from 'react';
import { Box, IconButton, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { useBuildingMap } from '@shared/map/ui/BuildingMapContext';
import * as turf from '@turf/turf';
import { GeoJSON } from 'leaflet';
import { isFeature } from '@shared/types/geoJsonTypeGuards';

interface ObjectDescriptionPopupProps {
  children: ReactNode;
  onClose: () => void;
  focusedDescription: any;
  title: string;
}

export default function ObjectDescriptionPopup({
    children,
    focusedDescription,
    title,
    onClose
}: ObjectDescriptionPopupProps) {
  const { map } = useBuildingMap();
  const popupRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleMapClick(e: L.LeafletMouseEvent) {
      const target = e.originalEvent.target as Node;
      const clickWasInsideThePopup = popupRef.current && popupRef.current.contains(target);
      if (clickWasInsideThePopup) return;

      if (isFeature(focusedDescription)) {
        const clickWasInsideFeature = turf.booleanContains(focusedDescription, turf.point(GeoJSON.latLngToCoords(e.latlng)));
        if (clickWasInsideFeature) return;
      }

      onClose();
    }

    map.on('click', handleMapClick);
    map.on('levelpicker:changelevel', onClose);

    return () => {
      map.off('click', handleMapClick);
      map.off('levelpicker:changelevel', onClose);
    };
  }, [map, focusedDescription, onClose]);

  return (
    <Box
      ref={popupRef}
      onClick={(e) => e.stopPropagation()}
      sx={{
        position: 'absolute',
        top: '80px',
        right: '10px',
        width: '250px',
        border: '1px solid #ccc',
        borderRadius: '8px',
        padding: '10px',
        backgroundColor: 'white',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        zIndex: 1000,
      }}
    >
      <Box display='flex' justifyContent='flex-end'>
        <IconButton onClick={onClose} size='small'>
          <CloseIcon />
        </IconButton>
      </Box>
      <Typography variant='h6' gutterBottom>
        {title}
      </Typography>
      {children}
    </Box>
  );
}