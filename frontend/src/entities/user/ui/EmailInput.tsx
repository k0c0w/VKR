import { User } from "@entities/user/models/User";
import { TextField } from "@mui/material";
import { Control, Controller } from "react-hook-form";


interface EmailInputProps {
  control: Control<User>;
  name: "email";
  label: string;
  disabled: boolean;
}

export function EmailInput({ disabled, control, name, label }: EmailInputProps) {
  return (
    <Controller
      name={name}
      control={control}
      rules={{
        required: "Почта обязательна",
        pattern: { value: /^\S+@\S+$/i, message: "Неверный формат" },
      }}
      render={({ field, fieldState: { error } }) => (
        <TextField
          {...field}
          type="email"
          label={label}
          error={!!error}
          disabled={disabled}
          helperText={error?.message}
          margin="dense"
          fullWidth
        />
      )}
    />
  );
}