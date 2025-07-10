import { User } from "@entities/user/models/User";
import { Autocomplete, Chip, ChipPropsColorOverrides, TextField, useTheme } from "@mui/material";
import { Control, Controller } from "react-hook-form";
import { getRoleColor } from "../lib/styling";
import { OverridableStringUnion } from "@mui/types";

interface RoleInputProps {
  control: Control<User>;
  name: "roles";
  options: string[];
}

type color = OverridableStringUnion<'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning', ChipPropsColorOverrides>;

export function RoleInput({ control, name, options }: RoleInputProps) {
  const theme = useTheme();
  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <Autocomplete
          multiple
          options={options}
          value={field.value}
          onChange={(e, newValue) => field.onChange(newValue)}
          renderTags={(value, getTagProps) =>
            value.map((option, index) => (
              <Chip label={option} sx={{color: getRoleColor(theme, option)}} color={getRoleColor(theme, option) as color} {...getTagProps({ index })} />
            ))
          }
          renderInput={(params) => (
            <TextField {...params} label="Роли" margin="dense" fullWidth />
          )}
        />
      )}
    />
  );
}