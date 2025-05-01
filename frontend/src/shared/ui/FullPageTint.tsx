import React from 'react';
import { Box } from '@mui/material';

interface FullPageTintProps {
  children: React.ReactNode;
  tintColor?: string;
  opacity?: number;
}

const FullPageTint: React.FC<FullPageTintProps> = ({
  children,
  tintColor = 'rgba(0, 0, 0, 0.5)',
  opacity = 0.8,
}) => {
  return (
    <Box
      sx={{
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100vw',
        height: '100vh',
        backgroundColor: tintColor,
        opacity: opacity,
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        zIndex: 9999,
      }}
    >
      {children}
    </Box>
  );
};

export default FullPageTint;