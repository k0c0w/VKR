import { Room, RoomMetaProperties, RoomType } from "@entities/map";
import { FormControl, InputLabel, MenuItem, Select, SelectChangeEvent, TextField } from "@mui/material";
import { useState } from "react";

export interface RoomDescriptorProps {
    room: Room;
    belongsTo: {
        levelName: string;
    };
    onMetaPropsChange: (updatedMetaProps: RoomMetaProperties) => void;
    errors?: {
        id?: boolean;
        type?: boolean;
        name?: boolean;
    }
}

export default function RoomDescriptor({room, onMetaPropsChange, belongsTo, errors}: RoomDescriptorProps) {
    const [metaProps, setMetaProps] = useState<RoomMetaProperties>({
        name: room.properties.name ?? "",
        id: room.properties.id ?? "",
        type: room.properties.type,
        meaning: room.properties.meaning
    });

    const handleInputChange = (field: keyof RoomMetaProperties, value: string) => {
        const updatedMetaProps = { ...metaProps, [field]: value };
        setMetaProps(updatedMetaProps);
        onMetaPropsChange(updatedMetaProps);
    };

    const onSelectChange = (e: SelectChangeEvent<RoomType>) => {
        const targetValue = e.target.value;
        const strVal = typeof targetValue === "string" ? targetValue : RoomType[targetValue];
        if (strVal === ""){
            return;
        }

        handleInputChange("type", strVal);
    }

    return (
        <>
            <FormControl error={errors?.type} fullWidth sx={{ mb: 2 }}>
                <InputLabel id="space-label">Тип помещения</InputLabel>
                <Select labelId="space-label" value={metaProps.type} label="Space" onChange={onSelectChange}>
                    {Object.values(RoomType).map((val, i) => <MenuItem key={i} value={val}>{val}</MenuItem>)}
                </Select>
            </FormControl>
            <TextField
                fullWidth
                label="Наименование помещения"
                error={errors?.name}
                value={metaProps.name}
                onChange={(e) => handleInputChange("name", e.target.value)}
                sx={{ mb: 2 }}
            />
            <TextField
                fullWidth
                label="Индетификатор помещения"
                error={errors?.id}
                value={metaProps.id}
                onChange={(e) => handleInputChange("id", e.target.value)}
                sx={{ mb: 2 }}
            />
            {/* Non-Editable Belongs To Fields */}
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
