import { plansApi } from "@features/plans";
import { Box, Button, Container, Typography } from "@mui/material";
import { AvailableAddressesTableSkeletonWidget } from "@widgets/address";
import AvailablePlansPageContent from "./AvailablePlansPageContent";
import ErrorMessage from "@shared/ui/ErrorMessage";

export default function AvailablePlansPage() {
    const {data, isLoading, isSuccess, isError, error, refetch} = plansApi.useGetAvailablePlansListQuery();

    return <>
        <title>Доступные планы</title>
        <Container component="main">
            {isLoading && <AvailableAddressesTableSkeletonWidget/>}
            {isSuccess && data && <AvailablePlansPageContent data={data}/>}
            {isError && error && 
                <ErrorMessage>
                    <Box>
                        <Typography>Произошла ошибка при загрузке</Typography>
                    </Box>
                    <Button variant="outlined" onClick={refetch}>
                        Повторить загрузку
                    </Button>
                </ErrorMessage>
            }
        </Container>
    </>
}