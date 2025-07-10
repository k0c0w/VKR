import { plansApi } from "@features/plans";
import { Box, Button, Container, Grid, Stack, Typography } from "@mui/material";
import AvailablePlansPageContent from "./AvailablePlansPageContent";
import ErrorMessage from "@shared/ui/ErrorMessage";
import { AvailablePlansPageContentSkeleton } from "./AvailablePlansPageSkeleton";
import CreateNewPlanButton from "./CreateNewPlanButton";

export default function AvailablePlansPage() {
  const { data, isFetching, isSuccess, isError, error, refetch } = plansApi.useGetAvailablePlansListQuery();

  return (
    <>
      <title>Доступные планы</title>
      <Container
        component="main"
        maxWidth="xl"
        sx={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          px: { mobile: 2, tablet: 3 },
        }}
      >
        <Box sx={{ width: '100%', maxWidth: '1000px', mx: 'auto', py: 2 }}>
          {/* Title and Add Button Block */}
          <Grid
            container
            alignItems="center"
            justifyContent="space-between"
            sx={{ mb: 3 }}
          >
            <Grid size={{ mobile: 12, tablet: 'auto' }}>
              <Typography variant="h5">Созданные планы</Typography>
            </Grid>
            <CreateNewPlanButton />
          </Grid>

          {/* Table Block */}
          <Box sx={{ minHeight: '400px' }}>
            {isFetching && <AvailablePlansPageContentSkeleton />}
            {isSuccess && data && <AvailablePlansPageContent data={data} />}
            {isError && error && (
              <Stack
                sx={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  height: '100%',
                  minHeight: "50vh"
                }}
              >
                <ErrorMessage>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography>Произошла ошибка при загрузке</Typography>
                  </Box>
                  <Button variant="outlined" onClick={refetch}>
                    Повторить загрузку
                  </Button>
                </ErrorMessage>
              </Stack>
            )}
          </Box>
        </Box>
      </Container>
    </>
  );
}