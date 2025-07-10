import { AppBar, Button, CircularProgress, Dialog, IconButton, Slide, Stack, Toolbar, Typography } from "@mui/material";
import { TransitionProps } from "@mui/material/transitions";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import CloseIcon from '@mui/icons-material/Close';
import React, { useEffect } from "react";
import { itEquipmentCardApi } from "../api/itEquipmentCardApi";

const Transition = React.forwardRef(function Transition(
  props: TransitionProps & {
    children: React.ReactElement<unknown>;
  },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

interface CatalogueItEquipmentCardProps {
  url: string;
  cardName: string;
  open: boolean;
  onClose: () => void;
}

export default function CatalogueItEquipmentCard({ url, cardName, open, onClose }: CatalogueItEquipmentCardProps) {
  const user = useAppSelector(state => state.authSlice.currentUser);
  const [fetch, {data, isFetching, error, isSuccess, isUninitialized, reset }] = itEquipmentCardApi.useLazyFetchCardHtmlQuery();

  const retry = () => {
    if (!isUninitialized) {
      reset();
    }
    fetch(url);
  }

  useEffect(() => {
    if (open) {
      fetch(url);
    } else if (!isUninitialized) {
      reset();
    }
  }, [url, open, isUninitialized, reset]);

  if (user === undefined) {
    // On unauthorized
    return <></>;
  }

  return (
    <Dialog
        fullScreen
        open={open}
        slots={{transition: Transition}}
    >
      <AppBar sx={{ position: 'relative' }}>
          <Toolbar>
              <IconButton
                edge="start"
                color="inherit"
                onClick={onClose}
                aria-label="close"
              >
                <CloseIcon />
              </IconButton>
              <Typography sx={{ ml: 2, flex: 1 }} variant="h6" component="div">
                  {cardName}
              </Typography>
          </Toolbar>
      </AppBar>
      {isSuccess && data && <iframe srcDoc={data.data} width="100%" height="100%" />}
      {!isSuccess && <Stack sx={{flexDirection: "column", justifyContent: "center", alignItems: "center", height:"100%"}}>
          {isFetching && <CircularProgress />}
          {error && <>
            <Typography variant="body1" color="error">
              Ошибка при загрузке страницы
            </Typography>
            {data && data.status === 404 && <Typography variant="body1" color="error">
                Информация об объекте не найдена
              </Typography>}
            <Button variant="outlined" size="medium" sx={{mT: 2}} onClick={retry} >Повторить загрузку</Button>
          </>}
        </Stack>
      }
    </Dialog>
  );
}