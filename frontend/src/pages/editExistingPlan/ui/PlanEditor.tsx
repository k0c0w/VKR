import { planResponseToBuilding, plansApi } from "@features/plans";
import { Container, Stack, CircularProgress } from "@mui/material";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { PlanEditorStepperWidget, PlanEditorWidget } from "@widgets/editor";
import { useCallback, useEffect, useState } from "react";
import { generateUpdateInstructions } from "../lib/utils";
import FullPageTint from "@shared/ui/FullPageTint";
import AlertDialog from "@shared/ui/AlertDialog";
import { useNavigate } from "react-router";
import { Routes } from "@app/routing/routes";
import { isDomainErrorResponse, isProblemDetatils, isServerErrorResponse, isValidationErrorResponse } from "@shared/types/ProblemDetails";
import PlanHeader from "@shared/ui/PlanHeader";
import { Building } from "@entities/map";

export default function PlanEditor({id}: {id: string}) {
    const building = useAppSelector(x => x.planEditorSlice.building);
    if (!building) {
        throw new Error("Initialize planEditor slice first!");
    }

    const [initialBuilding] = useState(building);
    const [proccessing, setProccessing] = useState(false);
    const [error, setError] = useState('');

    const navigate = useNavigate();
    const goBack = useCallback((building: Building) => navigate(Routes.FormatSpecificPlanRouteTemplate(id), {replace: true, state: {building}}), [navigate]);
    const goToLogin = useCallback(() => navigate(Routes.SignInRouteTemplate), [navigate]);

    const [patchPlan, {data, isSuccess, isLoading, isError, error: patchError}] = plansApi.useUpdatePlanMutation();

    const onEditingComplete = () => {
        setProccessing(true);
        const updateInstructions = generateUpdateInstructions(initialBuilding, building);
        if (updateInstructions.length === 0) {
            setProccessing(false); // Ensure proccessing is reset if no updates
            goBack(initialBuilding);
            return;
        }

        patchPlan({id, updateInstructions});
    };

    useEffect(() => {
        if (isSuccess && data) {
            setProccessing(false);
            const building = planResponseToBuilding(data);
            goBack(building);
        }
    }, [isSuccess, data, goBack]);

    useEffect(() => {
        if (isLoading) {
            setProccessing(true);
        } else if (isError || isSuccess) {
            setProccessing(false);
        }

        if (isError && patchError) {
            if ('data' in patchError && isProblemDetatils(patchError.data)) {
                if (isDomainErrorResponse(patchError)) {
                    setError(patchError.data.detail ?? "Ошибка при изменении плана.");
                } else if (isValidationErrorResponse(patchError)) {
                    setError(`Ошибки валидации: ${JSON.stringify(patchError.data.errors)}`);
                } 
                else if (isServerErrorResponse(patchError)) {
                    setError("Сервер вернул 500 статус код.");
                } else if (patchError.data.status === 401 || patchError.data.status === 403) {
                    setProccessing(false);
                    goToLogin();
                    return;
                } else {
                    setError("Произошла ошибка, которую клиент не смог обработать.");
                }
            } else {
                setError("Сетевая ошибка.");
            }
        }
    }, [isError, isLoading, isSuccess, patchError, goToLogin]);

    return <>
        <Container maxWidth="xl" disableGutters fixed sx={{width: "100vw", height: "100vh"}}>
            <Stack gap={2} sx={{ width:"100%", height:"100%" }}>
                <PlanHeader name={building.properties.name} address={building.properties.address} />
                <PlanEditorWidget />
                <PlanEditorStepperWidget
                    backwardButtonDisabled={proccessing || isLoading}
                    completeButtonDisabled={proccessing || isLoading}
                    onComplete={onEditingComplete}
                />
            </Stack>
            {(proccessing || isLoading) && <FullPageTint>
                <CircularProgress />
            </FullPageTint>}
            {error && <AlertDialog title="Ошибка обновления плана"
                handleClose={() => setError('')}
                open={error !== ''}
            >
                {error}
            </AlertDialog>}
        </Container>
    </>;
}