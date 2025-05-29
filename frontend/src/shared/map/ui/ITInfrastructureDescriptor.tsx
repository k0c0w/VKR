import { ITInfrastructure, ITInfrastructureMetaProperties } from "@entities/map";
import { TextField } from "@mui/material";
import { useState } from "react";

const emptyAction = () => {};

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
    };
    editable: boolean;
}

export default function ITInfrastructureDescriptor({ infrastructure, belongsTo, onMetaPropsChange, errors, editable }: ITInfrastructureDescriptorProps) {
    const [metaProps, setMetaProps] = useState<ITInfrastructureMetaProperties>({
        ...infrastructure.properties,
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
                disabled={!editable}
                label="Инвентарный номер"
                error={errors?.inventoryNumber}
                value={metaProps.inventoryNumber}
                onChange={editable ? (e) => handleInputChange("inventoryNumber", e.target.value) : emptyAction}
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                required
                disabled={!editable}
                label="Наименование"
                error={errors?.name}
                value={metaProps.name}
                onChange={editable ? (e) => handleInputChange("name", e.target.value) : emptyAction}
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                required
                disabled={!editable}
                label="Серийный номер"
                error={errors?.serialNumber}
                value={metaProps.serialNumber}
                onChange={editable ? (e) => handleInputChange("serialNumber", e.target.value) : emptyAction}
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