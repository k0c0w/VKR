import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useNavigate } from 'react-router-dom';
import { AvailableAddressModel } from '../model/AvailableAddressModel';
import { Box, Button, IconButton, Stack, Typography } from '@mui/material';
import { Routes } from '@app/routing/routes';
import { guid } from '@shared/types/guid';
import DeleteForeverOutlinedIcon from '@mui/icons-material/DeleteForeverOutlined';
import AlertDialog from '@shared/ui/AlertDialog';
import { useState } from 'react';

interface AvailableAddressesTableWidgetProps {
  addresses: AvailableAddressModel[];
  paginationModel: {pageSize: number; page: 0;}
  pageSizeOptions: number[];
  onRemoveAddressClicked: (buildingId: guid) => void;
}

export default function AvailableAddressesTableWidget({ addresses, paginationModel, pageSizeOptions, onRemoveAddressClicked }: AvailableAddressesTableWidgetProps) {
  const navigate = useNavigate();
  const [removeAddressId, setRemoveAddressId] = useState<guid|undefined>();
  const removePlan = () => {
    if (removeAddressId) {
        onRemoveAddressClicked(removeAddressId);
        setRemoveAddressId(undefined);
    }
  }
  const rows = addresses.map((address) => ({
    id: address.buildingId,
    address: address.address,
  }));

  const columns: GridColDef<{ id: string; address: string }>[] = [
        { field: 'address', headerName: 'Адрес', flex: 1 },
        {
            field: 'action-open',
            headerName: '',
            sortable: false,
            filterable: false,
            align: 'center',
            headerAlign: 'center',
            renderCell: (params: { row: { id: guid; }; }) => (
              <IconButton color='error' size='small' onClick={()=>setRemoveAddressId(params.row.id)}>
                  <DeleteForeverOutlinedIcon/>
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
            renderCell: (params: { row: { id: guid; }; }) => (
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
    <div style={{ maxWidth: 650, margin: '16px auto' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        pageSizeOptions={pageSizeOptions}
        initialState={{
          pagination: { paginationModel: paginationModel},
        }}
        disableRowSelectionOnClick
        sx={{ '& .MuiDataGrid-root': { border: 'none' } }}
      />
      <AlertDialog
        open={removeAddressId !== undefined}
        handleClose={() => setRemoveAddressId(undefined)}
        title={removeAddressId ? `Оставить заявку на удаление ${addresses.find(x => x.buildingId === removeAddressId)?.address}?`: ""}
      >
        <Button variant='outlined' color='error' size="large" onClick={removePlan}>Да</Button>
      </AlertDialog>
    </div>
  );
};