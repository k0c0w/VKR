import { useAppDispatch, useAppSelector, } from "@shared/hooks/reduxTypedHooks";
import { useEffect, useState } from "react";
import { useMap } from "react-leaflet";
import { addLevelAndSwitchOnIt, editCurrentLevel, removeCurrentLevel, setCurrentLevelIndex } from "./planEditorSlice";
import { Button, TextField } from "@mui/material";
import DialogForm from "@shared/map/ui/DialogForm";

function LevelNameDialog({open, handleClose, setLevelName}: {open: boolean; handleClose: () => void; setLevelName: (name: string) => void}) {
    const onSubmit = (formJson: {[k: string]: any}) => {
      const levelName = formJson["levelName"] as string;
      if (!levelName) {
        return;
      }

      setLevelName(levelName);
      handleClose();
    };

    return <DialogForm
        onFormSubmit={onSubmit}
        handleClose={handleClose}
        open={open}
        title="Изменить этаж"
        helperText="Введите название этажа. Название этажа и номер этажа могут отличаться."
        inputs={
          <TextField
          autoFocus
          required
          margin="dense"
          name="levelName"
          label="Название этажа"
          fullWidth
          variant="standard"
        />
        }
        buttons={<>
            <Button onClick={handleClose} variant="outlined">Отменить</Button>
            <Button type="submit" variant="contained">Добавить</Button>
          </>}
      />
}

function RemoveLevelDialog({open, handleClose, onRemoveSubmit}: {open: boolean; handleClose: () => void; onRemoveSubmit: () => void;}) {
  return <DialogForm
      open={open}
      handleClose={handleClose}
      onFormSubmit={onRemoveSubmit}
      inputs={<></>}
      buttons={<>
          <Button onClick={() => {onRemoveSubmit(); handleClose();}} variant="outlined">Удалить</Button>
          <Button type="submit" variant="contained">Отменить</Button>
      </>}
      title="Удаление этажа"
      helperText="Удаление этажа приведет к потере всей разметки этажа. Вы уверены, что хотите удалить этаж?"
    />
}

function EditLevelDialog({open, handleClose, setLevelName}: {open: boolean; handleClose: () => void; setLevelName: (name: string) => void;}) {
  const {building, currentLevelIndex} = useAppSelector(state => state.planEditorSlice);
  const [newLevelName, setNewLevelName] = useState(building!.properties.levels[currentLevelIndex].name);

  const onSubmit = (formJson: {[k: string]: any}) => {
    const levelName = formJson["levelName"] as string;
    if (!levelName) {
      return;
    }

    setLevelName(levelName);
    handleClose();
  };

  return <DialogForm
    title="Изменить информацию об этаже"
    handleClose={handleClose}
    onFormSubmit={onSubmit}
    open={open}
    inputs={
      <TextField
        autoFocus
        required
        margin="dense"
        name="levelName"
        label="Название этажа"
        fullWidth
        variant="standard"
        value={newLevelName}
        onChange={(e) => setNewLevelName(e.target.value)}
      />
    }
    buttons={<>
      <Button onClick={() => handleClose()} variant="outlined">Отменить</Button>
      <Button type="submit" variant="contained">Сохранить</Button>
    </>}
    />
}

export default function LevelPickController() {
    const [levelNameDialogIsOpen, setLevelNameDialogOpen] = useState(false);
    const [editLeveDialogIsOpen, setEditLevelDialogIsOpen] = useState(false);
    const [removeLevelDialogIsOpen, setRemoveLevelDialogIsOpen] = useState(false);

    const map = useMap();
    const dispatch = useAppDispatch();

    function addLevel(levelName: string) {
        dispatch(addLevelAndSwitchOnIt({name: levelName}));
    }

    function renameLevel(levelName: string) {
        dispatch(editCurrentLevel({levelName}));
    }

    useEffect(() => {
        const onAddLevelClicked = () => setLevelNameDialogOpen(true);
        const switchLevel = ({levelIndex}: {levelIndex: number;}) => dispatch(setCurrentLevelIndex(levelIndex));
        const onRemoveLevelClicked = () => setRemoveLevelDialogIsOpen(true);
        const onEditLevelClicked = () => {
          setEditLevelDialogIsOpen(true);
        }

        map.on("levelpicker:changelevel", switchLevel);
        map.on("levelpicker:addbuttonclicked", onAddLevelClicked);
        map.on("levelpicker:removelevelclicked", onRemoveLevelClicked);
        map.on("levelpicker:editlevelbuttonclicked", onEditLevelClicked);

        return () => {
            map.off("levelpicker:changelevel", switchLevel);
            map.off("levelpicker:addbuttonclicked", onAddLevelClicked);
            map.off("levelpicker:removelevelclicked", onRemoveLevelClicked);
            map.off("levelpicker:editlevelbuttonclicked", onEditLevelClicked);
        }
    }, [map, dispatch, setLevelNameDialogOpen, setRemoveLevelDialogIsOpen, setEditLevelDialogIsOpen]);

    return <>
        <LevelNameDialog 
            open={levelNameDialogIsOpen}
            handleClose={() => setLevelNameDialogOpen(false)}
            setLevelName={(name) => addLevel(name)}
        />
        <RemoveLevelDialog 
            open={removeLevelDialogIsOpen}
            handleClose={() => setRemoveLevelDialogIsOpen(false)}
            onRemoveSubmit={() => dispatch(removeCurrentLevel())}
        />
        <EditLevelDialog
          open={editLeveDialogIsOpen}
          handleClose={() => setEditLevelDialogIsOpen(false)}
          setLevelName={renameLevel}
        />
    </>
}