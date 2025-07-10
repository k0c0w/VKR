import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useNavigate } from 'react-router-dom';
import { AvailableAddressModel } from '../../../pages/viewAvailablePlans/lib/AvailableAddressModel';
import { Box, Button, IconButton } from '@mui/material';
import { Routes } from '@app/routing/routes';
import guid from '@shared/types/guid';
import DeleteForeverOutlinedIcon from '@mui/icons-material/DeleteForeverOutlined';
import { useState } from 'react';
import ConfirmationDialog from '@shared/ui/ConfirmationDialog';

interface AvailableAddressesTableWidgetProps {
  addresses: AvailableAddressModel[];
  paginationModel: { pageSize: number; page: 0 };
  pageSizeOptions: number[];
  onRemoveAddressClicked: (buildingId: guid) => void;
}

export default function AvailableAddressesTableWidget({ addresses, paginationModel, pageSizeOptions, onRemoveAddressClicked }: AvailableAddressesTableWidgetProps) {
  const navigate = useNavigate();
  const [removeAddressId, setRemoveAddressId] = useState<guid | undefined>();
  const removePlan = () => {
    if (removeAddressId) {
      onRemoveAddressClicked(removeAddressId);
      setRemoveAddressId(undefined);
    }
  };
  const rows = addresses.map((address) => ({
    id: address.buildingId,
    buildingName: address.buildingName,
    address: address.address,
  }));

  const columns: GridColDef<{ id: string; address: string }>[] = [
    { field: 'buildingName', headerName: 'Наименование', flex: 1, sortable: true, },
    { field: 'address', headerName: 'Адрес', flex: 2 },
    {
      field: 'action-open',
      headerName: '',
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: { row: { id: guid } }) => (
        <IconButton color="error" size="small" onClick={() => setRemoveAddressId(params.row.id)}>
          <DeleteForeverOutlinedIcon />
        </IconButton>
      ),
    },
    {
      field: 'action-remove',
      headerName: '',
      sortable: false,
      filterable: false,
      align: 'right',
      headerAlign: 'right',
      width: 150,
      renderCell: (params: { row: { id: guid } }) => (
        <Button
          variant="contained"
          color="primary"
          size="small"
          onClick={() => navigate(Routes.FormatSpecificPlanRouteTemplate(params.row.id))}
        >
          Открыть план
        </Button>
      ),
    },
  ];

  return (
    <Box style={{ width: '100%', margin: '16px 0' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        pageSizeOptions={pageSizeOptions}
        initialState={{
          pagination: { paginationModel: paginationModel },
        }}
        disableRowSelectionOnClick
        sx={{ '& .MuiDataGrid-root': { border: 'none' }, width: '100%' }}
      />
      <ConfirmationDialog
        open={removeAddressId !== undefined}
        onClose={() => setRemoveAddressId(undefined)}
        onConfirm={removePlan}
        onDecline={() => setRemoveAddressId(undefined)}
        question={`Удалить ${addresses.find(x => x.buildingId === removeAddressId)?.address}?`}
      />
    </Box>
  );
};