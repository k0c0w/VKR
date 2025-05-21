import { ReactNode } from 'react';
import { Stack, Button, Box } from '@mui/material';

interface ErrorMessageProps {
  info: ReactNode; 
  action?: {
    onClick: () => void;
    text: string;
  };
  maxWidth?: number | string;
}

export default function ErrorMessage({ info, action, maxWidth = 650 }: ErrorMessageProps) {
  return (
    <Box sx={{ maxWidth, mx: 'auto', my: 4, minHeight: '50vh' }}>
      <Stack
        direction="column"
        spacing={2}
        alignItems="center"
        justifyContent="center"
        sx={{ height: '100%', textAlign: 'center' }}
      >
        <Box>{info}</Box>
        {action && (
          <Button variant="outlined" color="primary" onClick={action.onClick}>
            {action.text}
          </Button>
        )}
      </Stack>
    </Box>
  );
};