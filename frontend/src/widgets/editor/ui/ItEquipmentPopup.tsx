import { useState } from 'react';
import { Divider, Grid, Link, TextField } from '@mui/material';
import { isITEquipment } from '@entities/map/models/ItEquipment';
import ObjectDescriptionPopup from './ObjectDescriptionPopup';
import CatalogueItEquipmentCard from '@entities/catalogue/ui/CatalogueItEquipmentCardDialog';
import { usePlanViewerContext } from './PlanViewerContext';
import { useBuildingMap } from '@shared/map';
import { isRoom } from '@entities/map';

interface ItEquipmentPopupProps {
}

export default function ItEquipmentPopup({}: ItEquipmentPopupProps) {
  const { building } = useBuildingMap();
  const { focusedFeature, unfocus } = usePlanViewerContext();
  const [openEquipmentCard, setOpenEquipmentCard] = useState(false);
  const [openHistoryCard, setOpenHistoryCard] = useState(false);

  if (!focusedFeature || !isITEquipment(focusedFeature)) {
    return <></>
  }
  
  const equipment = focusedFeature;
  const roomId = equipment.properties.linkedToAudienceId;
  const room = building.properties.levels
                .flatMap(level => level.buildingStructure)
                .filter(x => isRoom(x))
                .find(r => r.id == roomId);

  //@ts-ignore
  const roomName = room?.properties?.name ?? 'Неизвестно';

  return (
    <ObjectDescriptionPopup focusedDescription={focusedFeature} title='ИТ-оборудование' onClose={unfocus}>
      <TextField
          value={equipment.properties.name}
          disabled
          label='Наименование'
          variant='outlined'
          sx={{mb: 2}}
          fullWidth
      />
      <TextField
        value={equipment.id}
        disabled
        label='Инвентарный номер'
        variant='outlined'
        sx={{mb: 2}}
        fullWidth
      />
      <Grid container spacing={2} sx={{mb: 2}}>
        <Grid size={12}>
          <Link onClick={() => setOpenEquipmentCard(true)}>
            Полная инвентарная карточка
          </Link>
        </Grid>
        {/*<Grid size={6}>
          <Link onClick={() => setOpenHistoryCard(true)}>
            История изменений
          </Link>
        </Grid>
        */}
      </Grid>
      <Divider/>
      <TextField
        value={roomName}
        disabled
        label='Место установки'
        variant='outlined'
        sx={{mb: 2}}
        fullWidth
      />
  
      <CatalogueItEquipmentCard 
          cardName='Инвентарная карточка' 
          url={equipment.properties.cardUrl} 
          open={openEquipmentCard} 
          onClose={() => setOpenEquipmentCard(false)} 
      />
      <CatalogueItEquipmentCard 
          cardName='История изменений' 
          url={equipment.properties.historyUrl} 
          open={openHistoryCard} 
          onClose={() => setOpenHistoryCard(false)} 
      />
    </ObjectDescriptionPopup>
  );
}