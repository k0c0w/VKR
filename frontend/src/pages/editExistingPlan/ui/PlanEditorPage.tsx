import { Routes } from "@app/routing/routes";
import { Building } from "@entities/map";
import { unauthenticateUser } from "@features/authorization";
import { catalogueApi } from "@features/catalogues";
import { planResponseToBuilding, plansApi } from "@features/plans";
import { Container } from "@mui/material";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { isNotFoundErrorResponse, isProblemDetatils } from "@shared/types/ProblemDetails";
import PageContainer from "@shared/ui/PageContainer";
import PlanPageLoadErrorContent from "@shared/ui/PlanPageLoadErrorContent";
import { CreateNewPlanStep, initStateWithBuilding, resetToInitialState, setStep } from "@widgets/editor";
import { useEffect } from "react";
import { Navigate, useLocation, useNavigate, useParams } from "react-router"
import PlanEditor from "./PlanEditor";
import CenteredCircularProgress from "@shared/ui/CenteredCircularProgression";

export default function EditExistingPlanPage() {
    const {id} = useParams();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const location = useLocation();
    const building = useAppSelector(s => s.planEditorSlice.building);

    const {data, isSuccess, isError, isFetching, error, refetch} = plansApi.useGetSpecificPlanQuery({id: id ?? ""});
    const [fetchCatalogue, { data: catlogueData, isSuccess: isCatalogueSuccess, isFetching: isCatalogueFetching, isError: isCatalogueError, error: catalogueError }] = catalogueApi.useLazyGetBuildingQuery();

    const retry = () => {
        if (isError) {
            refetch();
        }

        if (isSuccess && data && isCatalogueError) {
            fetchCatalogue({
                address: data.address,
                name: data.buildingName,
                region: data.region
            });
        }
    }

    useEffect(() => {
        dispatch(resetToInitialState());
    }, []);

    useEffect(() => {
        if (isSuccess && data && !(isCatalogueFetching || isCatalogueSuccess)) {
            fetchCatalogue({region: data.region, address: data.address, name: data.buildingName});
            return;
        }

        if (isSuccess && isCatalogueSuccess && data && catlogueData) {
            const fetchedBuilding: Building = planResponseToBuilding(data);

            dispatch(initStateWithBuilding({ building: fetchedBuilding, catalogue:catlogueData }))
            dispatch(setStep(CreateNewPlanStep.InfrastructureSetup));
        }

    }, [data, isSuccess, catlogueData, isCatalogueSuccess, isCatalogueFetching, isCatalogueSuccess, fetchCatalogue]);

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

        if (isCatalogueError && catalogueError) {
            if ('data' in catalogueError && isProblemDetatils(catalogueError.data) 
                && (catalogueError.data.status === 401 || catalogueError.data.status === 403)) {
                navigate(Routes.SignInRouteTemplate, {
                    state: {from: location}
                });
                return;
            }
        }
    }, [isError, error, isCatalogueError, catalogueError, dispatch]);

    if (!id) {
        return <Navigate to={Routes.AvailablePlansRouteTemplate} replace/>
    }

    return <>
        <title>Редактирование плана</title>
        <Container component="main" maxWidth={false} disableGutters>
            {isSuccess && building && <PlanEditor id={id} />}
            {!building && <PageContainer sx={{display: "flex", flexDirection:"column", justifyContent:"center", minHeight:600}}>
                    {(isFetching || isCatalogueFetching) && <CenteredCircularProgress />}
                    {(isError || isCatalogueError) && <PlanPageLoadErrorContent retry={retry} />}
            </PageContainer>}
        </Container>
    </>
}
