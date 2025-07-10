import { Autocomplete, FormControl, InputLabel, MenuItem, Select, SelectChangeEvent, TextField } from '@mui/material';
import { useBuildingMap } from '@shared/map';
import { isRoom, Room, RoomType } from '@entities/map';
import guid from '@shared/types/guid';
import ObjectDescriptionPopup from './ObjectDescriptionPopup';
import { usePlanViewerContext } from './PlanViewerContext';
import { CatalogueRoom } from '@entities/catalogue';

export interface CatalogueBuilding {
  name: string;
  address: string;
  levels: { name: string; rooms: { name: string; roomId: number }[] }[];
}

interface RoomDescriptionPopupProps {
  catalogue?: CatalogueBuilding;
  onUpdate?: ({ level, prevId, update }: { level: number; prevId: guid | number; update: Partial<Room> }) => void;
}

export default function RoomDescriptionPopup({
  catalogue,
  onUpdate,
}: RoomDescriptionPopupProps) {
  const { focusedFeature, unfocus, focusOn } = usePlanViewerContext();
  const { building, currentLevelIndex } = useBuildingMap();

  if (!focusedFeature || !isRoom(focusedFeature)) {
    return <></>
  }

  const usedRoomIds = new Set<number>(building.properties
    .levels[currentLevelIndex]
    .buildingStructure
    .filter((f) => isRoom(f))
    .map((f) => Number(f.id)) || []);

  let availableRooms: CatalogueRoom[] = [];
  let currentFeatureFromCatalogue: CatalogueRoom | undefined;
  if (catalogue && catalogue.levels.length > 0 && catalogue.levels[currentLevelIndex]?.rooms) {
    for(const cR of catalogue.levels[currentLevelIndex].rooms) {
      if (!usedRoomIds.has(cR.roomId)) {
        availableRooms.push(cR)
      }
      if (cR.roomId === focusedFeature.id) {
        currentFeatureFromCatalogue = cR;
      }
    }
  } else if (!catalogue) {
    currentFeatureFromCatalogue = {
      name: focusedFeature.properties.name ?? "",
      roomId: focusedFeature.id!,
    }
  }

  if (currentFeatureFromCatalogue && !availableRooms.some(x => x.roomId === currentFeatureFromCatalogue?.roomId)) {
    availableRooms.push(currentFeatureFromCatalogue);
  }

  const handleRoomChange = (newRoom: { name: string; roomId: number } | null) => {
    if (!focusedFeature || !onUpdate || !newRoom) return;

    const newProps = {
      ...focusedFeature.properties,
      name: newRoom.name
    };

    onUpdate({
      level: currentLevelIndex,
      prevId: focusedFeature.id,
      update: {
        id: newRoom.roomId,
        properties: newProps
      }
    });

    const newFocus: Room = {
      ...focusedFeature,
      id: newRoom.roomId,
      properties: newProps
    };
    focusOn(newFocus);
  };

  const handleTypeChange = (e: SelectChangeEvent<RoomType>) => {
    if (!onUpdate || !focusedFeature) return;
    const newType = e.target.value as RoomType;
    const newProps = {
      ...focusedFeature.properties,
      type: newType,
    };
    const newFocus = {
      ...focusedFeature,
      properties: newProps
    };

    onUpdate({
      level: currentLevelIndex,
      prevId: focusedFeature.id,
      update: {
        properties: newProps,
      },
    });
    focusOn(newFocus);
  };

  const disabled = onUpdate === undefined;

  if (!currentFeatureFromCatalogue) {
    currentFeatureFromCatalogue = {
      name: "Не задано",
      roomId: 0
    }
  }

  return (
    <ObjectDescriptionPopup onClose={unfocus} focusedDescription={focusedFeature} title='Комната'>
        <FormControl fullWidth sx={{ mb: 2 }}>
        <InputLabel id='space-label'>Тип помещения</InputLabel>
        <Select
          disabled={disabled}
          labelId='space-label'
          value={focusedFeature.properties.type}
          label='Тип помещения'
          onChange={handleTypeChange}
        >
          {Object.values(RoomType).map((val, i) => (
            <MenuItem key={i} value={val}>{val}</MenuItem>
          ))}
        </Select>
      </FormControl>
      <Autocomplete
        options={availableRooms}
        getOptionLabel={(option) => option.name}
        value={currentFeatureFromCatalogue}
        onChange={(_event, value) => handleRoomChange(value)}
        disabled={disabled}
        disableClearable
        renderInput={(params) => (
          <TextField
            {...params}
            label='Наименование помещения'
            variant='outlined'
            size='small'
            sx={{ mb: 2 }}
          />
        )}
        isOptionEqualToValue={(option, value) => option.roomId === value.roomId}
        fullWidth
      />
      <Autocomplete
        options={availableRooms}
        getOptionLabel={(option) => option.roomId.toString()}
        value={currentFeatureFromCatalogue}
        disabled
        renderInput={(params) => (
          <TextField
            {...params}
            label='Идентификатор помещения'
            variant='outlined'
            size='small'
            sx={{ mb: 2 }}
          />
        )}
        isOptionEqualToValue={(option, value) => option.roomId === value.roomId}
        fullWidth
      />
      <TextField
        fullWidth
        label='Этаж'
        value={building.properties.levels[currentLevelIndex].name}
        disabled
        sx={{ mb: 2 }}
      />
    </ObjectDescriptionPopup>
  );
}