import { Routes } from "@app/routing/routes";
import { Building } from "@entities/map";
import { unauthenticateUser } from "@features/authorization";
import { planResponseToBuilding, plansApi } from "@features/plans";
import { Container } from "@mui/material";
import { useAppDispatch } from "@shared/hooks/reduxTypedHooks";
import { isNotFoundErrorResponse } from "@shared/types/ProblemDetails";
import CenteredCircularProgress from "@shared/ui/CenteredCircularProgression";
import PageContainer from "@shared/ui/PageContainer";
import PlanPageLoadErrorContent from "@shared/ui/PlanPageLoadErrorContent";
import { useEffect, useState } from "react";
import { Navigate, useLocation, useNavigate, useParams } from "react-router"
import PlanViewer from "./PlanViewer";

export default function PlanPage() {
    const {id} = useParams();
    const [building, setBuilding] = useState<Building | undefined>();
    const [queryPlan, {data, isSuccess, isError, isFetching, error, isUninitialized, reset}] = plansApi.useLazyGetSpecificPlanQuery();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const location = useLocation();

    const refetch = () => {
        if (!isUninitialized) {
            reset();
        }

        if (id) {
            queryPlan({id});
        }
    }

    // decide whether take building from location or fetch it
    useEffect(() => {
        if (location.state && 'building' in location.state) {
            const buildingFromState = location.state.building as Building;
            setBuilding(buildingFromState);
        } else if (id) {
            queryPlan({id})
        }

    }, [location]);

    // on data fetched
    useEffect(() => {
        if (isSuccess && data) {
            const building = planResponseToBuilding(data);
            setBuilding(building);
        }

    }, [setBuilding, data, isSuccess,]);

    // fetch error
    useEffect(() => {
        if (isError && error) {
            if (isNotFoundErrorResponse(error)) {
                navigate(Routes.AvailablePlansRouteTemplate);
                return;
            }
            if ('data' in error && error.status === 401) {
                
                navigate(Routes.SignInRouteTemplate, {
                    state: {
                        from: location
                    }
                });
                dispatch(unauthenticateUser());
                return;
            }
        }
    }, [isError, error, dispatch]);
    
    if (!id) {
        return <Navigate to={Routes.AvailablePlansRouteTemplate} replace/>
    }

    return <>
        <title>Просмотр плана</title>
        <Container component="main" maxWidth={false} disableGutters>
            {building && <PlanViewer building={building} />}
            {!building && !isSuccess && 
                <PageContainer sx={{display: "flex", flexDirection:"column", justifyContent:"center", minHeight:600}}>
                    {!building && isFetching &&  <CenteredCircularProgress />}
                    {isError && error && <PlanPageLoadErrorContent retry={refetch} />}
                </PageContainer>
            }
        </Container>
    </>
}
