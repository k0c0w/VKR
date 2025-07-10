import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogTitle from '@mui/material/DialogTitle';
import { Action } from '@shared/types/Action';


interface ConfirmationDialogProps {
    open: boolean;
    question: string;
    onConfirm: Action<void>;
    onDecline: Action<void>;
    onClose: () => void;
}

export default function ConfirmationDialog({open, question, onConfirm, onDecline, onClose}: ConfirmationDialogProps) {

  const yes = () => {
    onClose();
    onConfirm();
  }

  const no = () => {
    onClose();
    onDecline();
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
    >
      <DialogTitle id="alert-dialog-title">
        {question}
      </DialogTitle>
      <DialogActions>
        <Button variant="text" onClick={no}>Нет</Button>
        <Button variant="text" onClick={yes} autoFocus>
          Да
        </Button>
      </DialogActions>
    </Dialog>
  );
}
