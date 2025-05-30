import { Address, parseAddress } from "@shared/types/ValueObjectsTypes";
import { ReactNode, useEffect, useState } from "react";
import { Button, Container, Stack, Typography } from "@mui/material";
import { Building } from "@entities/map/Building";
import { mapApi, isBuildingValidationErrorResponse, isBuildingSuccessResponse } from "@features/map";
import { Level } from "@entities/map";
import {AddressFormWidget} from "@widgets/address";
import { isDomainErrorResponse, isNotFoundErrorResponse } from "@shared/types/ProblemDetails";

interface LoadBuildingWidgetProps {
    setBuilding: (building: Building) => void;
    loaderBackground?: ReactNode
}

function getDefaultBuilding(address: Address, name: string): Building {
    return {
        type: "Feature",
        geometry: {
            type: "Polygon",
            coordinates: [[[49.1040367, 55.7946956], [49.1028492, 55.7942629], [49.1021096, 55.7949044], [49.1032970, 55.7953371], [49.1040367, 55.7946956]]]
        },
        properties: {
            levels: [{number: 1, name: "1 этаж", buildingStructure: [], infrastructure: []}],
            address,
            name,
        }
    };
}

export default function LoadBuildingBoundariesSubPage({setBuilding}:LoadBuildingWidgetProps) {
    const [address, setAddress] = useState<Address | undefined>();
    const [fetchAddress, {data, isSuccess, isFetching, isLoading, isError, error}] = mapApi.useLazyFetchBuildingBoundariesQuery();
    const [fatalError, setFatalError] = useState<string|null>(null);
    const [fromErrors, setFormErrors] = useState<{city?: string[]; street?: string[]; house?: string[]}>({});

    const onSubmit = (address: Address) => {
        setAddress(address);
        fetchAddress({
            city: address.city,
            house: address.houseNumber,
            street: address.street
        });
    }

    const onManualBuildingCreation = () => {
        if (address) {
            setBuilding(getDefaultBuilding(address, "Новое Здание"));
        }
    }

    useEffect(() => {
        if (isSuccess && data && isBuildingSuccessResponse(data) && address) {
            const {geometry, levelsCount, address:addressFromServer} = data;
            const addressVO = parseAddress(addressFromServer);
            const levels: Level[] = Array.from({length: levelsCount})
            .map((_, x) => ({
                buildingStructure: [],
                infrastructure: [],
                name: `${x + 1} этаж`,
                number: x + 1,
            }) as Level);

            const building: Building = {
                type: "Feature",
                geometry: geometry,
                properties: {
                    levels: [...levels],
                    address: addressVO ?? address,
                    name: "Новое Здание"
                }
            };

            setBuilding(building);
        } else if (isError) {
            if (error && isBuildingValidationErrorResponse(error)) {
                const {errors} = error.data;
                setFormErrors({
                    city: errors.City,
                    house: errors.House,
                    street: errors.Street
                });
            } else if (error && isDomainErrorResponse(error)) {
                setFatalError(error.data?.detail ?? "Произошла непредвиденная ошибка.");
            } else if (error && isNotFoundErrorResponse(error)) {
                setFatalError("Здание по указанному адресу не найдено.");
            } else {
                setFatalError("Произошла непредвиденная ошибка.");
            }
        }
    }, [data, error, isSuccess, isError]);

    useEffect(() => {
        if (isFetching || isLoading) {
            setFormErrors({});
            setFatalError(null);
        }
    }, [isFetching, isLoading]);

    return <Container maxWidth="tablet">
        <AddressFormWidget 
            onSubmit={onSubmit} 
            disabled={isFetching} 
            externalErrors={fromErrors} 
            submitButtonVariant={fatalError == null ? "contained" : "outlined"}
        />
        {fatalError !== null && <Stack>
            <Typography color="error">{fatalError}</Typography>
            <Button variant="contained" disabled={address === undefined} onClick={onManualBuildingCreation}>
                Создать здание самостоятельно
            </Button>
        </Stack>}
    </Container>
}