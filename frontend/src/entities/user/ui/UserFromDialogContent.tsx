import { DialogContent, Typography } from "@mui/material";
import { EmailInput } from "./EmailInput";
import { RoleInput } from "./RoleInput";
import { Control } from "react-hook-form";
import { User } from "../models/User";

export default function UserFormDialogContent({error, disableEmail, formControl}: {
    error?: string; formControl: Control<User>; disableEmail?: boolean;}) {

    return <>
        <DialogContent>
            {error && (
              <Typography color="error" sx={{ mb: 2 }}>
                {error}
              </Typography>
            )}
            <EmailInput disabled={disableEmail ?? false} control={formControl} name="email" label="Эл. адрес" />
            <RoleInput
              control={formControl}
              name="roles"
              options={["editor", "root", "moderator"]}
            />
            <Typography variant="caption" color="textSecondary" sx={{ mt: 1 }}>
              Роль 'user' всегда включена.
            </Typography>
        </DialogContent>
      </>
    }