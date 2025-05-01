import { Button, InputLabel, MenuItem, Paper, Select, TextField, Typography } from "@mui/material";
import { Address } from "@shared/types/ValueObjectsTypes";
import { useForm } from "react-hook-form";
import { Form } from "react-router";

interface AddressFormProps {
    onSubmit(address: Address): void;
}

export default function AddressForm({onSubmit}:AddressFormProps) {
    const {register, handleSubmit} = useForm<Address>();

    function onSubmitHandler(address: Address) {
        alert(address);
        /* TODO: добавить валидацию полей и триммить их через резолвер */
        onSubmit({
            city: "Kazan",
            houseNumber: "18",
            street: "Kremlyovskaya"
        });
    }

    return <Paper>
        <Typography component="h1" variant="h5">
            Адрес
        </Typography>
        <Form noValidate onSubmit={handleSubmit(onSubmitHandler)}>
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
            >
              <MenuItem value="Казань">Казань</MenuItem>
            </Select>
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
                helperText="Например, Кремлёвская"
                autoFocus
            />
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
            />
            <Button
                type="submit"
                fullWidth
                variant="contained"
                color="primary"
            >
                Загрузить границы здания
            </Button>
        </Form>
    </Paper>
}