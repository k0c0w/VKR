import { Button, InputLabel, MenuItem, Paper, Select, TextField, Typography } from "@mui/material";
import { Address } from "@shared/types/ValueObjectsTypes";
import { useForm } from "react-hook-form";
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { Form } from "react-router";

const schema = yup.object().shape({
    city: yup.string().trim().required("Город обязателен.").min(2, "Город должен содержать минимум 2 символа."),
    street: yup.string().trim().required("Улица обязательна.").test('street-format', 'Улица должна содержать минимум 2 слова.', value => {
        return value.split(/\s+/).filter(word => word).length >= 2;
    }),
    houseNumber: yup.string().trim().required("Номер дома обязателен.").test('houseNumber-format', 'Неверный формат дома.', value => {
        return /^\d+$/.test(value) || /^\d+\/\d+$/.test(value) || /^(\d+\/?\d*)([а-яА-Я]\d*)*$/.test(value);
    }),
});

interface AddressFormProps {
    onSubmit(address: Address): void;
    externalErrors?:{
        city?: string[];
        street?: string[];
        house?: string[];
    };
    disabled: boolean;
    submitButtonVariant?: "outlined" | "contained";
}

export default function AddressForm({onSubmit, externalErrors, disabled, submitButtonVariant = "contained"}:AddressFormProps) {
    const {register, handleSubmit, formState:{errors: formErrors}} = useForm<Address>({
        resolver: yupResolver(schema),
        mode: 'onChange',
    });

    const combinedErrors = {
        city: [
            ...(formErrors.city ? [formErrors.city.message] : []),
            ...(externalErrors?.city || []),
        ].filter(Boolean),
        street: [
            ...(formErrors.street ? [formErrors.street.message] : []),
            ...(externalErrors?.street || []),
        ].filter(Boolean),
        houseNumber: [
            ...(formErrors.houseNumber ? [formErrors.houseNumber.message] : []),
            ...(externalErrors?.house || []),
        ].filter(Boolean),
    };

    return <Paper>
        <Typography component="h1" variant="h5">
            Адрес
        </Typography>
        <Form noValidate onSubmit={handleSubmit(onSubmit)}>
            <InputLabel id="city-select-label">Город</InputLabel>
            <Select
              {...register("city", {
                  required: true,
              })}
              labelId="city-select-label"
              id="city"
              label="Город"
              required
              fullWidth
              defaultValue="Казань"
              name="city"
              disabled={disabled}
              error={combinedErrors.city.length > 0}
            >
              <MenuItem value="Казань">Казань</MenuItem>
            </Select>
            {combinedErrors.city.map((error, index) => (
              <Typography
                  key={`city-error-${index}`}
                  color="error"
                  variant="caption"
                  display="block"
              >
                  {error}
              </Typography>
            ))}
            <TextField
                {...register("street", {
                    required: true,
                })}
                variant="outlined"
                margin="normal"
                fullWidth
                id="street"
                label="Улица"
                name="street"
                helperText="Например, улица Кремлёвская"
                autoFocus
                disabled={disabled}
                error={combinedErrors.street.length > 0}
            />
            {combinedErrors.street.map((error, index) => (
                <Typography
                    key={`street-error-${index}`}
                    color="error"
                    variant="caption"
                    display="block"
                >
                    {error}
                </Typography>
            ))}
            <TextField
                {...register("houseNumber", {
                    required: true
                })}
                variant="outlined"
                margin="normal"
                required
                fullWidth
                id="houseNumber"
                label="Дом"
                name="houseNumber"
                helperText="Например, 18к1"
                disabled={disabled}
                error={combinedErrors.houseNumber.length > 0}
            />
            {combinedErrors.houseNumber.map((error, index) => (
                <Typography
                    key={`house-error-${index}`}
                    color="error"
                    variant="caption"
                    display="block"
                >
                    {error}
                </Typography>
            ))}
            <Button
                type="submit"
                fullWidth
                variant={submitButtonVariant}
                color="primary"
                disabled={disabled || Object.keys(formErrors).length > 0}
            >
                Загрузить границы здания
            </Button>
        </Form>
    </Paper>
}