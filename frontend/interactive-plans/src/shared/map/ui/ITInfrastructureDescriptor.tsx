import { ITInfrastructure, ITInfrastructureMetaProperties } from "@entities/map";
import { TextField } from "@mui/material";
import { useState } from "react";

export interface ITInfrastructureDescriptorProps {
    infrastructure: ITInfrastructure;
    belongsTo: {
        roomName: string;
        levelName: string;
    };
    onMetaPropsChange: (updatedMetaProps: ITInfrastructureMetaProperties) => void;
    errors?: {
        name?: boolean;
        inventoryNumber?: boolean;
        serialNumber?: boolean;
    }
}

export default function ITInfrastructureDescriptor({ infrastructure, belongsTo, onMetaPropsChange, errors }: ITInfrastructureDescriptorProps) {
    const [metaProps, setMetaProps] = useState<ITInfrastructureMetaProperties>({
        name: infrastructure.properties.name,
        inventoryNumber: infrastructure.properties.inventoryNumber,
        serialNumber: infrastructure.properties.serialNumber,
        meaning: infrastructure.properties.meaning,
    });

    const handleInputChange = (field: keyof ITInfrastructureMetaProperties, value: string) => {
        const updatedMetaProps = { ...metaProps, [field]: value };
        setMetaProps(updatedMetaProps);
        onMetaPropsChange(updatedMetaProps);
    };

    return (
        <>
            <TextField
                fullWidth
                required
                label="Инвентарный номер"
                error={errors?.inventoryNumber}
                value={metaProps.inventoryNumber}
                onChange={(e) => handleInputChange("inventoryNumber", e.target.value)}
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                required
                label="Наименование"
                error={errors?.name}
                value={metaProps.name}
                onChange={(e) => handleInputChange("name", e.target.value)}
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                required
                label="Серийный номер"
                error={errors?.serialNumber}
                value={metaProps.serialNumber}
                onChange={(e) => handleInputChange("serialNumber", e.target.value)}
                sx={{ mb: 2 }}
            />
            {/* Non-Editable Belongs To Fields */}
            <TextField
                fullWidth
                label="Комната"
                value={belongsTo.roomName}
                disabled
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                label="Этаж"
                value={belongsTo.levelName}
                disabled
                sx={{ mb: 2 }}
            />
        </>
    );
}