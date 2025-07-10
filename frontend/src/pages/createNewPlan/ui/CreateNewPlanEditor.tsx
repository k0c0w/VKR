import { mapRoomType, plansApi } from "@features/plans";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Building, isRoom } from "@entities/map";
import guid from "@shared/types/guid";
import { Stack } from "@mui/material";
import { PlanEditorStepperWidget, PlanEditorWidget, validatePlanState } from "@widgets/editor";
import { ReactNode, useEffect, useState } from "react";
import { LineString, Point, Polygon } from "geojson";
import { isDomainErrorResponse, isProblemDetatils, isServerErrorResponse, isValidationErrorResponse, isValidationProblemDetails } from "@shared/types/ProblemDetails";
import { useLocation, useNavigate } from "react-router-dom";
import { Routes } from "@app/routing/routes";
import PlanHeader from "@shared/ui/PlanHeader";

interface CreateNewPlanEditorProps {
  onPlanSucsessfulCreationCallback: (createdPlanId: guid) => void;
  onError: (error: { title: string; payload: ReactNode }) => void;
}

export default function CreateNewPlanEditor({ onPlanSucsessfulCreationCallback, onError }: CreateNewPlanEditorProps) {
  const { building, buildingInfoFromCatalogue} = useAppSelector(state => state.planEditorSlice);
  const [createButtonDisabled, setCreateButtonDisabled] = useState(false);
  const [validating, setValidating] = useState(false);
  const [createNewPlan, { data, error, isLoading, isSuccess, isError, reset, isUninitialized }] = plansApi.useCreateNewPlanMutation();
  const navigate = useNavigate();
  const location = useLocation();

  if (building === undefined) {
    throw new Error("Initialize slice first!");
  }

  function onEditingComplete() {
    setValidating(true);

    const {isValid, error} = validatePlanState(building!, {rooms: buildingInfoFromCatalogue.levels.flatMap(x => x.rooms)})
    if (isValid) {
      createPlan(building!);
    } else {
      setCreateButtonDisabled(true);
      onError({ title: "Невалидное состояние плана", payload: error });
    }

    setValidating(false);
  }

  function createPlan(building: Building) {
    const { geometry, properties } = building;

    if (!isUninitialized && isError) {
      reset();
    }

    createNewPlan({
      ...properties,
      basementGeometry: geometry,
      buildingName: properties.name,
      levels: properties.levels.map(l => ({
        name: l.name,
        number: l.number,
        structure: l.buildingStructure.map(x => {
          const geometry = x.geometry;
          if (isRoom(x)) {
            return {
              id: x.id as number,
              name: x.properties.name ?? "",
              geometry: geometry as Polygon,
              type: mapRoomType(x.properties.type),
              meaning: "Room",
            } as {
              id: number;
              name: string;
              geometry: Polygon;
              type: number;
              meaning: "Room";
            };
          }

          return {
            geometry: geometry as LineString,
            meaning: "Wall",
          } as { geometry: LineString; meaning: "Wall" };
        }),
        itEquipments: l.infrastructure.map(x => ({locationPoint: x.geometry, 
          inventoryNumber: x.id.toString(), 
          relatedToRoomId:x.properties.linkedToAudienceId
          } as {locationPoint: Point; inventoryNumber: string; relatedToRoomId: number;}   
        )),
      })),
    });
  }

  useEffect(() => {
    if (isSuccess && data) {
      onPlanSucsessfulCreationCallback(data.id);
    }
  }, [isSuccess, data, onPlanSucsessfulCreationCallback]);

  useEffect(() => {
    if (error) {
      let errorMessage = "Произошла ошибка при создании плана.";
      if ("data" in error && isProblemDetatils(error.data)) {
        if (isValidationProblemDetails(error.data)) {
          if (isValidationErrorResponse(error)) {
            errorMessage = 'Ошибки валаидации, проверьте план:' + JSON.stringify(error.data.errors);
          } else {
            errorMessage = error.data.details ?? "Ошибка валидации плана"
          }
        }
        else if (isDomainErrorResponse(error)) {
          if (error.status === 401 || error.status === 403) {
            navigate(Routes.SignInRouteTemplate, {state:{from: location}, replace: true});
            return;
          } else {
            errorMessage = error.data.detail ?? "Не удалось выполнить операцию."
          }
        }
        else if (isServerErrorResponse(error)) {
          errorMessage = error.data.detail ?? "При создании плана на сервере произошла ошибка, которая не была обработана."
        }

      }
      onError({ title: "Ошибка создания плана", payload: errorMessage });

      reset();
    }
  }, [error, onError, reset]);

  useEffect(() => {
    if (createButtonDisabled) {
      setCreateButtonDisabled(false);
    }
  }, [building]);

  return (
    <Stack gap={2} sx={{ width:"100%", height:"100%" }}>
      <PlanHeader name={building.properties.name} address={building.properties.address} />
      <PlanEditorWidget />
      <PlanEditorStepperWidget
        backwardButtonDisabled={validating || isLoading}
        completeButtonDisabled={createButtonDisabled || validating || isLoading}
        onComplete={onEditingComplete}
      />
    </Stack>
  );
}