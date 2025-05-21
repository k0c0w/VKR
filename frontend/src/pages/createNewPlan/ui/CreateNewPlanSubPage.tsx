import { Building, hasValidState } from "@entities/map";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import AlertDialog from "@shared/ui/AlertDialog";
import { PlanEditorWidget, PlanEditorStepperWidget } from "@widgets/editor";
import { useEffect, useState } from "react";

export default function CreateNewPlanSubPage({createPlan}: {createPlan: (b: Building) => void}) {
    const building = useAppSelector(state => state.planEditorSlice.building)!;
    const [createButtonDisabled, setCreateButtonDisabled] = useState(false);
    const [validating, setValidating] = useState(false);
    const [error, setError] = useState("");

    function onEditingComplete() {
        setValidating(true);
        
        if (!hasValidState(building)) {
            setCreateButtonDisabled(true);
            setError("Не валидное состояние");    
        } else {
            createPlan(building);
        }

        setValidating(false);
    }

    useEffect(() => {
        if (createButtonDisabled) {
            setCreateButtonDisabled(false);
        }
    }, [building]);

    return <>
        <PlanEditorWidget style={{width: 600, height: 800}} />
        <PlanEditorStepperWidget 
            backwardButtonDisabled={validating} 
            completeButtonDisabled={createButtonDisabled || validating} 
            onComplete={onEditingComplete}
        />
        <AlertDialog 
            title="Невалидное состояние плана"
            content={error}
            open={error !== ""}
            handleClose={() => setError("")}
        />
    </>
}