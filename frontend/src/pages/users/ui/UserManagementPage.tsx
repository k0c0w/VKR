import { Box, Button, Container, Grid, Typography } from "@mui/material";
import ErrorMessage from "@shared/ui/ErrorMessage";
import { AddUserWidget } from "@widgets/users";
import { userManagementApi } from "@features/users";
import AvailableUsersTable, { AvailableUsersTableSkeleton } from "./AvailableUsersTable";
import { isFetchBaseQueryError, isProblemDetatils } from "@shared/types/ProblemDetails";
import { useLocation, useNavigate } from "react-router-dom";
import { Routes } from "@app/routing/routes";
import { useEffect } from "react";

export default function UserManagementPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { data, isFetching, isSuccess, isError, error, refetch, isUninitialized } = userManagementApi.useGetUserListQuery();

  const refetchUserList = () => {
    if (!isUninitialized) {
        refetch();
    }
  }

  useEffect(() => {
    if (error && isFetchBaseQueryError(error) && isProblemDetatils(error.data) && (error.data.status === 401 || error.data.status ===403)) {
      navigate(Routes.SignInRouteTemplate, {state: {from: location, replace: true}});
      return;
    }
  }, [error, navigate, location]);

  return (
    <>
      <title>Управление пользователями</title>
      <Container
        component="main"
        maxWidth="xl"
        sx={{
          minHeight: "100vh",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          px: { mobile: 2, tablet: 3 },
        }}
      >
        <Box sx={{ width: "100%", maxWidth: "1000px", mx: "auto", py: 2 }}>
          <Grid container alignItems="center" justifyContent="space-between" sx={{ mb: 3 }}>
            <Grid>
              <Typography variant="h5">Пользователи системы</Typography>
            </Grid>
            <Grid>
              <AddUserWidget />
            </Grid>
          </Grid>

          <Box sx={{ minHeight: "400px" }}>
            {isFetching && <AvailableUsersTableSkeleton />}
            {!isFetching && isSuccess && data && <AvailableUsersTable users={data} refreshData={refetchUserList} />}
            {isError && error && (
              <Box sx={{ textAlign: "center", minHeight: "50vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <ErrorMessage>
                  <Typography>Произошла ошибка при загрузке</Typography>
                  <Button variant="outlined" onClick={refetchUserList}>
                    Повторить загрузку
                  </Button>
                </ErrorMessage>
              </Box>
            )}
          </Box>
        </Box>
      </Container>
    </>
  );
}