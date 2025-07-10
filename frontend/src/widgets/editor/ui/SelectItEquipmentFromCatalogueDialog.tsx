import React, { useEffect, useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  TextField,
  List,
  ListItem,
  ListItemText,
  IconButton,
  CircularProgress,
  Typography,
  Button,
  Box,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { catalogueApi } from '@features/catalogues';
import { CatalogueItEquipment } from '@entities/catalogue';
import { isDomainErrorResponse, isProblemDetatils, isServerErrorResponse, isValidationProblemDetails } from '@shared/types/ProblemDetails';
import { useLocation, useNavigate } from 'react-router';
import { Routes } from '@app/routing/routes';

interface ItEquipmentDialogProps {
  roomIds: number[];
  onSelected: (equipment: CatalogueItEquipment, roomId: number) => void;
  open: boolean;
  onClose: () => void;
  assignedEquipment: Set<number | string>;
}

export default function ItEquipmentDialog({ roomIds, onSelected, open, onClose, assignedEquipment }: ItEquipmentDialogProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const { data, isFetching, isError, error, refetch } = catalogueApi.useGetItEquipmentQuery({ roomIds });
  const [errorMsg, setErrorMsg] = useState('');
  const navigate = useNavigate();
  const location = useLocation();

  const filteredEquipment = React.useMemo(() => {
    if (!data) return [];
    return Object.entries(data)
      .flatMap(([roomId, equipment]) =>
        (equipment as CatalogueItEquipment[])
          .filter((item: CatalogueItEquipment) => 
            !assignedEquipment.has(item.id) &&
            (item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
            item.id.toString().includes(searchQuery))
          )
          .map((item: CatalogueItEquipment) => ({ ...item, roomId: Number(roomId) }))
      )
      .sort((a, b) => {
        if (a.roomId !== b.roomId) {
          return a.roomId - b.roomId;
        }
         return a.name.localeCompare(b.name); 
      });
  }, [data, searchQuery, assignedEquipment]);

  const retry = () => {setErrorMsg(""); refetch()};
  const handleSelect = (equipment: CatalogueItEquipment, roomId: number) => {
    onSelected(equipment, roomId);
    onClose();
  };

  useEffect(() => {
    if (!isError || !error) {
        return;
    }

    if (isValidationProblemDetails(error)) {
        setErrorMsg("Ошибка валидации данных, возможно клиент содержит ошибку.");
        return;
    }

    if (isDomainErrorResponse(error)) {
        setErrorMsg(error.data.detail ?? "Не удалось получить данные из каталога.");
        return;
    }

    if (isServerErrorResponse(error)) {
        setErrorMsg("Сервер не смог обработать запрос и вернул 500.");
    }

    if (isProblemDetatils(error) && error.status === 401) {
        navigate(Routes.SignInRouteTemplate, {state: {from: location}, replace: true});
        return;
    }
    
    setErrorMsg("Произошла ошибка при загрузке каталога.");
  }, [isError, error]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center',}}>
        Каталог ИТ-оборудования
        <IconButton onClick={onClose}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        {isFetching ? (
          <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
            <CircularProgress />
          </Box>
        ) : errorMsg ? (
          <Box display="flex" flexDirection="column" alignItems="center" minHeight={200}>
            <Typography variant="body1" color="error" gutterBottom>
              {errorMsg}
            </Typography>
            <Button variant="outlined" onClick={retry}>
              Повторить загрузку
            </Button>
          </Box>
        ) : (
          <>
            <TextField
              fullWidth
              label="Найти ИТ-оборудование"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              variant="outlined"
              size="small"
              sx={{ mt:1, mb: 2, position: 'sticky', top: 0, zIndex: 1, backgroundColor: 'white' }}
              autoFocus
            />
            <List sx={{ maxHeight: 300, overflowY: 'auto' }}>
              {filteredEquipment.length === 0 ? (
                <ListItem>
                  <ListItemText primary="Нет доступного ИТ-оборудования" />
                </ListItem>
              ) : (
                filteredEquipment.map((item) => (
                  <ListItem
                    key={`${item.roomId}-${item.id}`}
                    onClick={() => handleSelect(item, item.roomId)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <ListItemText primary={item.name} secondary={`ID: ${item.id}, Room ID: ${item.roomId}`} />
                  </ListItem>
                ))
              )}
            </List>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}