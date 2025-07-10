import { User,getRoleColor } from "@entities/user";
import { Box, IconButton, Typography, useTheme } from "@mui/material";
import { DataGrid, GridColDef, GridSortModel } from "@mui/x-data-grid";
import { Action } from "@shared/types/Action";
import EditIcon from '@mui/icons-material/Edit';
import { useState } from "react";

interface UsersTableWidgetProps {
  users: User[];
  paginationModel: { pageSize: number; page: 0 };
  pageSizeOptions: number[];
  onUserSelect: Action<User>;
}

type GridRow = { id: string; } & User;

export default function UsersTableWidget({ users, paginationModel, pageSizeOptions, onUserSelect }: UsersTableWidgetProps) {
    const theme = useTheme();
    const [sortModel, setSortModel] = useState<GridSortModel>([
      {
        field: 'email',
        sort: 'asc',
      },
    ]);

    const rows = users.map((user) => ({
      id: user.email,
      ...user,
    }));

  const columns: GridColDef<GridRow>[] = [
    { field: 'email', headerName: 'Почта', flex: 1, sortable: true, renderCell: (params: {row: GridRow}) => (
        <Box><Typography component="span">{params.row.email}</Typography></Box>
    )},
    { field: 'roles', headerName: 'Роли', flex: 2, sortable: false,  renderCell: (params: {row: GridRow}) =>(
        <Box>
          {params.row.roles.map(role => <Typography component="span" mr={0.5} sx={{color: getRoleColor(theme, role)}}>
              {role}
          </Typography>)}
        </Box>
    )},
    {
      field: 'action-open',
      headerName: '',
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: { row: GridRow }) => (
        <IconButton size="small" onClick={() => onUserSelect({email: params.row.email, roles: params.row.roles})}>
          <EditIcon />
        </IconButton>
      ),
    }
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
        sortModel={sortModel}
        onSortModelChange={(newSortModel) => setSortModel(newSortModel)}
        disableRowSelectionOnClick
        sx={{ '& .MuiDataGrid-root': { border: 'none' }, width: '100%' }}
      />
    </Box>
  );
};