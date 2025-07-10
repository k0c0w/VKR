import { useEffect, useState } from "react";
import { Button, Stack, Typography } from "@mui/material";
import { mapApi } from "@features/map";
import { catalogueApi } from "@features/catalogues";
import { isDomainErrorResponse, isNotFoundErrorResponse } from "@shared/types/ProblemDetails";
import { CatalogueBuilding } from "@entities/catalogue";
import { Polygon } from "geojson";
import CenteredCircularProgress from "@shared/ui/CenteredCircularProgression";

interface LoadBuildingStartInformationProps {
  buildingInfo: { region: string; address: string; name: string };
  onLoadComplete: ({ basementGeometry, buildingInfo, region }: {
    basementGeometry: Polygon;
    buildingInfo: CatalogueBuilding;
    region: string;
}) => void;
}

const defaultGeometry: Polygon = {
  type: "Polygon",
  coordinates: [[[49.1040367, 55.7946956], [49.1028492, 55.7942629], [49.1021096, 55.7949044], [49.1032970, 55.7953371], [49.1040367, 55.7946956]]],
};

export default function LoadBuildingStartInformation({ buildingInfo, onLoadComplete }: LoadBuildingStartInformationProps) {
  const [boundariesQuery, { data: boundariesData, isSuccess: isBoundariesSuccess, isFetching: isBoundariesFetching, isLoading: isBoundariesLoading, isError: isBoundariesError, error: boundariesError, isUninitialized: isBoundariesUninitialized, reset:resetBoundaries }] = mapApi.useLazyFetchBuildingBoundariesQuery();
  const [buildingQuery, { data: buildingData, isSuccess: isBuildingSuccess, isFetching: isBuildingFetching, isLoading: isBuildingLoading, isError: isBuildingError, error: buildingError, isUninitialized: isBuildingUnitialized, reset:resetBuilding }] = catalogueApi.useLazyGetBuildingQuery();
  const [fatalError, setFatalError] = useState<string | null>(null);

  const fetchBoundaries = () => boundariesQuery({ address: buildingInfo.address });
  const fetchBuilding = () => buildingQuery({ region: buildingInfo.region, name: buildingInfo.name, address: buildingInfo.address });

  const retry = () => {
    if (!isBoundariesUninitialized && isBoundariesError) {
      resetBoundaries();
      fetchBoundaries();
    }

    if (!isBuildingUnitialized && isBuildingError) {
      resetBuilding();
      fetchBuilding();
    }
  }

  // Fetch boundaries and building details on mount
  useEffect(() => {
    if (!isBuildingUnitialized) {
      resetBuilding();
    }

    if (!isBoundariesUninitialized) {
      resetBoundaries();
    }
    fetchBoundaries();
    fetchBuilding();
  }, []);

  // Combine results when both API calls succeed
  useEffect(() => {
    if (isBoundariesSuccess && boundariesData && isBuildingSuccess && buildingData) {
      let geometry: Polygon;
      try {
        geometry = {
          type: boundariesData.geometry.type,
          coordinates: boundariesData.geometry.coordinates
        };
        L.polygon(geometry.coordinates.map(ring => ring.map(coords => L.GeoJSON.coordsToLatLng(coords as [number, number]))));
      }
      catch(err) {
        geometry = defaultGeometry;
      }

      onLoadComplete({basementGeometry: geometry, buildingInfo: buildingData, region: buildingInfo.region});
    } else if (isBoundariesError || isBuildingError) {
      let errorMessage = "Произошла непредвиденная ошибка.";
      if (boundariesError && isNotFoundErrorResponse(boundariesError)) {
        errorMessage = "Границы здания не найдены.";
      } else if (buildingError && isNotFoundErrorResponse(buildingError)) {
        errorMessage = "Здание по указанному адресу не найдено.";
      } else if (boundariesError && isDomainErrorResponse(boundariesError)) {
        errorMessage = boundariesError?.data?.detail ?? "Произошла непредвиденная ошибка.";
      } else if (buildingError && isDomainErrorResponse(buildingError)) {
        errorMessage = buildingError?.data?.detail ?? "Произошла непредвиденная ошибка.";
      }
      setFatalError(errorMessage);
    }
  }, [
    boundariesData,
    buildingData,
    isBoundariesSuccess,
    isBuildingSuccess,
    isBoundariesError,
    isBuildingError,
    boundariesError,
    buildingError,
    buildingInfo,
    onLoadComplete,
  ]);

  // Clear errors when fetching
  useEffect(() => {
    if (isBoundariesFetching || isBoundariesLoading || isBuildingFetching || isBuildingLoading) {
      setFatalError(null);
    }
  }, [isBoundariesFetching, isBoundariesLoading, isBuildingFetching, isBuildingLoading]);

  // Handle manual building creation
  const handleManualBuildingCreation = () => {
    if (!isBuildingSuccess || !buildingData) {
      return;
    }

    
    onLoadComplete({basementGeometry: defaultGeometry, buildingInfo: buildingData, region: buildingInfo.region});
  };

  return (<>
      {(isBoundariesFetching || isBoundariesLoading || isBuildingFetching || isBuildingLoading) && <CenteredCircularProgress/>}
          {fatalError && (
            <Stack spacing={3}>
              <Typography variant="h6" color="error" component="div" textAlign="center">{fatalError}</Typography>
              <Button variant="outlined" onClick={retry}>
                  Повторить загрузку
              </Button>
              {isBuildingSuccess && 
                <Button variant="contained" onClick={handleManualBuildingCreation}>
                  Создать здание самостоятельно
                </Button>
              }
            </Stack>
          )}
      </>
  );
}