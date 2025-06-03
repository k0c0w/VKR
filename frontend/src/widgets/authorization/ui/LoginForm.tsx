import { Card, Typography, Box, FormControl, TextField, FormLabel, Button, useTheme } from "@mui/material";
import KfuSiteMarkIcon from "@shared/ui/KfuSiteMarkIcon";
import { useEffect } from "react";
import { useForm, Controller } from "react-hook-form";

interface LoginFormProps {
  onSubmit: (args: { login: string; password: string }) => void;
  disabled: boolean;
  initialErrors?: {
    globalErrorMessage?: string;
    login?: string[];
    password?: string[];
  };
}

interface FormData {
  email: string;
  password: string;
}

export default function LoginForm({ onSubmit, disabled, initialErrors }: LoginFormProps) {
    const theme = useTheme();
    const { control, handleSubmit, setError, clearErrors, watch, formState: { errors } } = useForm<FormData>({
      defaultValues: {
        email: '',
        password: '',
      },
      resolver: async (data) => {
        const errors: Record<string, { type: string; message: string }> = {};
        if (!data.email || !/\S+@\S+\.\S+/.test(data.email)) {
          errors.email = { type: 'invalid', message: 'Пожалуйста, введите валидный адрес.' };
        }
        if (!data.password) {
          errors.password = { type: 'required', message: 'Поле обязательно.' };
        }
        return {
          values: Object.keys(errors).length ? {} : data,
          errors,
        };
      },
    });

    const formValues = watch();
    useEffect(() => {
      if (initialErrors?.login?.[0]) {
        setError('email', { type: 'server', message: initialErrors.login[0] });
      }
      if (initialErrors?.password?.[0]) {
        setError('password', { type: 'server', message: initialErrors.password[0] });
      }
    }, [initialErrors, setError]);

    useEffect(() => {
      clearErrors(['email', 'password']);
    }, [formValues.email, formValues.password, clearErrors]);
  
    const onFormSubmit = (data: FormData) => {
      onSubmit({ login: data.email, password: data.password });
    };
  
    const isSubmitDisabled = disabled || !!errors.email || !!errors.password;

    return (
      <Card variant="outlined"
          sx={{
              display: 'flex',
              flexDirection: 'column',
              alignSelf: 'center',
              width: '100%',
              padding: theme.spacing(4),
              gap: theme.spacing(2),
              margin: 'auto',
              boxShadow:'hsla(220, 30%, 5%, 0.05) 0px 5px 15px 0px, hsla(220, 25%, 10%, 0.05) 0px 15px 35px -5px',
              [theme.breakpoints.up('mobile')]: { maxWidth: '450px',},
              [theme.breakpoints.up('tablet')]: {maxWidth: '600px',},
              [theme.breakpoints.up('lg')]: {maxWidth: '700px',},
              [theme.breakpoints.up('xl')]: {maxWidth: '800px',},
          }}
      >
        <KfuSiteMarkIcon />
        <Typography
          component="h1"
          variant="h4"
          sx={{ width: '100%', fontSize: 'clamp(2rem, 10vw, 2.15rem)' }}
        >
          Вход
        </Typography>
        {initialErrors?.globalErrorMessage && (
          <Typography color="error" variant="body2" sx={{ mt: 1, textAlign: 'center' }}>
            {initialErrors.globalErrorMessage}
          </Typography>
        )}
        <Box
          component="form"
          onSubmit={handleSubmit(onFormSubmit)}
          noValidate
          sx={{
            display: 'flex',
            flexDirection: 'column',
            width: '100%',
            gap: 2,
          }}
        >
          <FormControl>
            <FormLabel htmlFor="email">Почта</FormLabel>
            <Controller
              name="email"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  id="email"
                  type="email"
                  placeholder="email@kpfu.ru"
                  autoComplete="email"
                  autoFocus
                  required
                  fullWidth
                  variant="outlined"
                  disabled={disabled}
                  error={!!errors.email}
                  helperText={errors.email?.message}
                />
              )}
            />
          </FormControl>
          <FormControl>
            <FormLabel htmlFor="password">Пароль</FormLabel>
            <Controller
              name="password"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  id="password"
                  type="password"
                  placeholder="••••••"
                  autoComplete="current-password"
                  required
                  fullWidth
                  variant="outlined"
                  disabled={disabled}
                  error={!!errors.password}
                  helperText={errors.password?.message}
                />
              )}
            />
          </FormControl>
          <Button
            type="submit"
            fullWidth
            variant="contained"
            disabled={isSubmitDisabled}
          >
            Войти
          </Button>
        </Box>
      </Card>
    );
}