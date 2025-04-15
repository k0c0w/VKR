import { Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { ReactNode } from "react";

interface DialogFormProps {
    open: boolean;
    handleClose: () => void;
    onFormSubmit: (formData: {[key: string]: any}) => void;
    title?: string;
    helperText?: string;
    inputs: ReactNode;
    buttons: ReactNode;
}
export default function DialogForm({open, handleClose, onFormSubmit, title, helperText, inputs, buttons}:DialogFormProps) {
    return <Dialog
        open={open}
        onClose={handleClose}
        slotProps={{
          paper: {
            component: 'form',
            onSubmit: (event: React.FormEvent<HTMLFormElement>) => {
              event.preventDefault();
              const formData = new FormData(event.currentTarget);
              const formJson = Object.fromEntries((formData as any).entries());
              onFormSubmit(formJson);
            },
          },
        }}
      >
        <DialogTitle>{title}</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {helperText}
          </DialogContentText>
          {inputs}
        </DialogContent>
        <DialogActions>
          {buttons}
        </DialogActions>
      </Dialog>
}