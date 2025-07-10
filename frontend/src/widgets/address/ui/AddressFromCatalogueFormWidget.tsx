import { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { Box, Typography, FormControl, InputLabel, Select, MenuItem, Autocomplete, TextField, Button, Skeleton, Alert } from '@mui/material';
import { catalogueApi } from '@features/catalogues';
import { plansApi } from '@features/plans';

interface FormData {
  region: string;
  building: {name: string; address: string;} | null;
}

interface AddressFromCatalogueFormWidgetProps {
  onSubmit: (data: { region: string; name: string; address: string }) => void;
}

export default function AddressFromCatalogueFormWidget({ onSubmit }: AddressFromCatalogueFormWidgetProps) {
 
  const { control, handleSubmit, watch, setError, clearErrors, reset, setValue } = useForm<FormData>({
    defaultValues: {
      region: 'Выберите регион',
      building: null,
    },
  });

  const [globalError, setGlobalError] = useState<string | null>(null);
  const selectedRegion = watch('region');

  const { data: plans, isLoading: isPlansLoading, error: plansError, refetch: refetchPlans } = plansApi.useGetAvailablePlansListQuery();
  const { data: regions, isLoading: isRegionsLoading, error: regionsError, refetch: refetchRegions } = catalogueApi.useGetRegionsQuery();
  const { data: buildings, isFetching: isBuildingsLoading, error: buildingsError, refetch: refetchBuildings } = catalogueApi.useGetBuildingsQuery(
    { region: selectedRegion },
    { skip: !selectedRegion || selectedRegion === 'Выберите регион' }
  );

  const filteredBuildings = buildings?.filter((building) => 
    !plans?.some((plan) => plan.region === selectedRegion && plan.address === building.address && plan.buildingName === building.name))
      .sort((a, b) => {
        return a.name.localeCompare(b.name);
      }) 
  || [];

  useEffect(() => {
    if (selectedRegion && selectedRegion !== 'Выберите регион') {
      setValue('building', null, { shouldValidate: false });
      refetchBuildings();
    }
  }, [selectedRegion, setValue, refetchBuildings]);

  useEffect(() => {
    if (regionsError) {
        setGlobalError('Ошибка при загрузке регионов');
    } else if (buildingsError) {
        setGlobalError('Ошибка при загрузке зданий');
    } else if (plansError) {
        setGlobalError('Ошибка при загрузке планов');
    } else {
        setGlobalError(null);
    }
  }, [regionsError, buildingsError, plansError]);

  const handleFormSubmit = (data: FormData) => {
    if (!data.region || data.region === 'Выберите регион') {
      setError('region', { message: 'Регион обязателен' });
      return;
    }
    if (!data.building) {
      setError('building', { message: 'Здание обязательно' });
      return;
    }
    clearErrors();
    onSubmit({
      region: data.region,
      name: data.building.name,
      address: data.building.address,
    });
  };

  const handleRetry = () => {
    reset({ region: 'Выберите регион', building: null });
    clearErrors(); 
    setGlobalError(null);
    if (plansError) {
        refetchPlans();
    }
    refetchRegions();
    if (selectedRegion && selectedRegion !== 'Выберите регион') {
      refetchBuildings();
    }
  };

  return (
    <Box
      sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        p: { xs: 2, sm: 3, md: 4 },
        boxSizing: 'border-box',
      }}
    >
      <Typography variant="h5" component="h2" fontWeight="medium">
        Выбрать из каталога
      </Typography>
      <FormControl fullWidth error={!!regionsError}>
        <InputLabel id="region-select-label">Регион</InputLabel>
        <Controller
          name="region"
          control={control}
          rules={{ required: 'Регион обязателен', validate: (value) => value !== 'Выберите регион' || 'Регион обязателен' }}
          render={({ field }) => (
            <Select
              {...field}
              labelId="region-select-label"
              label="Регион"
              disabled={isRegionsLoading}
              onChange={(e) => {
                field.onChange(e);
                clearErrors('building'); 
              }}
            >
              {(!selectedRegion || selectedRegion === "Выберите регион") && (
                <MenuItem value="Выберите регион">
                  <em>Выберите регион</em>
                </MenuItem>
              )}
              {regions?.map((region) => (
                <MenuItem key={region} value={region}>
                  {region}
                </MenuItem>
              ))}
            </Select>
          )}
        />
      </FormControl>

      {selectedRegion && selectedRegion !== 'Выберите регион' && <>
        <FormControl fullWidth>
          {isBuildingsLoading || isPlansLoading ? (
            <Skeleton variant="rectangular" height={56} />
          ) : (
            <Controller
              name="building"
              control={control}
              rules={{ required: 'Здание обязательно' }}
              render={({ field, fieldState: { error } }) => (
                <Autocomplete
                  options={filteredBuildings}
                  getOptionLabel={(option) => `${option.name} - ${option.address}`}
                  isOptionEqualToValue={(option, value) => option.name === value.name && option.address === value.address}
                  onChange={(_, value) => field.onChange(value)}
                  value={field.value}
                  disabled={!selectedRegion || selectedRegion === 'Выберите регион'}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Здание"
                      error={!!error}
                      helperText={error?.message || (!filteredBuildings.length && !isBuildingsLoading && !isPlansLoading ? 'Нет доступных зданий' : '')}
                      placeholder="Введите название или адрес"
                    />
                  )}
                  sx={{ mt: 2 }}
                />
              )}
            />
          )}
        </FormControl>
        {globalError && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                <Alert severity="error" sx={{ width: '100%' }}>
                    {globalError}
                </Alert>
                <Button
                    variant="outlined"
                    color="primary"
                    onClick={handleRetry}
                    sx={{ alignSelf: { xs: 'stretch', sm: 'flex-end' }, width: { xs: '100%', sm: 'auto' } }}
                >
                    Повторить
                </Button>
            </Box>
        )}
        {!isBuildingsLoading && (
            <Button
                variant="contained"
                color="primary"
                onClick={handleSubmit(handleFormSubmit)}
                disabled={isBuildingsLoading || isPlansLoading || !watch('building')}
                sx={{
                    mt: 2,
                    alignSelf: { xs: 'stretch', sm: 'flex-end' },
                    width: { xs: '100%', sm: 'auto' },
                }}
            >
                Подтвердить
            </Button>
        )}
      </>}
    </Box>
  );
};