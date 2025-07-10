import { User } from "@entities/user/models/User"
import { Box, Grid, IconButton, Skeleton } from "@mui/material"
import CachedIcon from '@mui/icons-material/Cached';
import { Action } from "@shared/types/Action";
import { EditUserWidget, UsersTableWidget } from "@widgets/users";
import { useState } from "react";
import SearchQueryFilterInput from "@shared/ui/SearchQueryFilterInput";
import { useFilterUsersByEmail } from "../lib/SearchUserInTableFilter";
import { DataGrid, GridColDef } from "@mui/x-data-grid";

interface AvailableUsersTableProps {
    users: User[];
    refreshData: Action
}

export default function AvailableUsersTable({users, refreshData}: AvailableUsersTableProps) {
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const { searchQuery, setSearchQuery, filteredUsers } = useFilterUsersByEmail(users);

    return <>
        {/* Region Selector and Search Input */}
        <Grid
          container
          spacing={2}
          sx={{ mb: 3, flexWrap: { mobile: 'wrap', tablet: 'nowrap' } }}
        >
          <Grid size={{ mobile: 11, tablet: 11, lg: 11, xl: 11 }} component="div">
            <SearchQueryFilterInput
              placeHolder="Email фильтр"
              searchQuery={searchQuery} 
              setSearchQuery={setSearchQuery} 
            />
          </Grid>
          <Grid size={{ mobile: 1, tablet: 1, lg: 1, xl: 1 }} component="div"  sx={{display: "flex", alignItems: "center"}}>
            <IconButton size="small" onClick={() => refreshData()}>
                <CachedIcon/>
            </IconButton>
          </Grid>
        </Grid>
        {/* Table with users */}
        <UsersTableWidget 
            users={filteredUsers} 
            onUserSelect={(user) => {setSelectedUser(user); console.log("User selected:", user);}} 
            paginationModel={{
                pageSize: 5,
                page: 0,
            }}
            pageSizeOptions={[5, 10, 25]}
        />
        {/* user editing */}
        {selectedUser && <EditUserWidget user={selectedUser} onClose={() => setSelectedUser(null)}/>}
    </>
}

interface UserTableSkeletonProps {
  pageSize?: number;
}

export function AvailableUsersTableSkeleton({ pageSize = 5 }: UserTableSkeletonProps) {
  const columns: GridColDef[] = [
    {
      field: "email",
      headerName: "Почта",
      flex: 1,
      sortable: true,
      renderCell: () => <Skeleton variant="text" width="80%" />,
    },
    {
      field: "roles",
      headerName: "Роли",
      flex: 2,
      renderCell: () => (
        <Box sx={{ display: "flex", gap: 1 }}>
          <Skeleton variant="rounded" width={60} height={20} />
          <Skeleton variant="rounded" width={60} height={20} />
        </Box>
      ),
    },
    {
      field: "action-open",
      headerName: "",
      sortable: false,
      filterable: false,
      align: "center",
      headerAlign: "center",
      renderCell: () => <Skeleton variant="circular" width={24} height={24} />,
    },
  ];

  // Generate skeleton rows
  const rows = Array.from({ length: pageSize }, (_, index) => ({
    id: `skeleton-${index}`,
  }));

  return (
    <Box sx={{ width: "100%", margin: "16px 0" }}>
      {/* Skeleton for search bar and refresh button */}
      <Grid
        container
        spacing={2}
        sx={{ mb: 3, flexWrap: { mobile: "wrap", tablet: "nowrap" } }}
      >
        <Grid size={{ mobile: 12, tablet: 9, lg: 9, xl: 9 }} component="div">
          <Skeleton variant="rectangular" height={40} />
        </Grid>
        <Grid size={{ mobile: 3, tablet: 3, lg: 3, xl: 3, }} component="div">
          <Skeleton variant="circular" width={40} height={40} />
        </Grid>
      </Grid>
      {/* Skeleton for DataGrid */}
      <DataGrid
        rows={rows}
        columns={columns}
        pageSizeOptions={[pageSize]}
        initialState={{
          pagination: { paginationModel: { pageSize, page: 0 } },
        }}
        disableRowSelectionOnClick
        sx={{ "& .MuiDataGrid-root": { border: "none" }, width: "100%" }}
      />
    </Box>
  );
}