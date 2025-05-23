import { ReactNode } from 'react';
import { Stack, Box } from '@mui/material';

interface ErrorMessageProps {
  children: ReactNode;
  maxWidth?: number | string;
} 

export default function ErrorMessage({ children, maxWidth = 650 }: ErrorMessageProps) {
  return (
    <Box sx={{ maxWidth, mx: 'auto', my: 4, minHeight: '50vh' }}>
      <Stack
        direction="column"
        spacing={2}
        alignItems="center"
        justifyContent="center"
        sx={{ height: '100%', textAlign: 'center' }}
      >
        {children}
      </Stack>
    </Box>
  );
};