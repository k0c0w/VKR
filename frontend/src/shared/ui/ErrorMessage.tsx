import { ReactNode } from 'react';
import { Stack } from '@mui/material';

interface ErrorMessageProps {
  children: ReactNode;
  maxWidth?: number | string;
} 

export default function ErrorMessage({ children, maxWidth = 650 }: ErrorMessageProps) {
  return (
      <Stack
        direction="column"
        spacing={2}
        alignItems="center"
        justifyContent="center"
        sx={{ height: '100%', textAlign: 'center', maxWidth, mx: 'auto', my: 4 }}
      >
        {children}
      </Stack>
  );
};