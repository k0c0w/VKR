import React from 'react';
import { TextField, Box } from '@mui/material';

interface SearchProps {
  searchQuery: string;
  setSearchQuery: (query: string) => void;
}

export default function SearchAddressInTableFormWidget({ searchQuery, setSearchQuery }: SearchProps) {
    const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => setSearchQuery(event.target.value);

    return (
      <Box sx={{ p: 2, maxWidth: 650, mx: 'auto' }}>
        <TextField
          label="Фильтр по адресу"
          variant="outlined"
          value={searchQuery}
          onChange={handleSearchChange}
          fullWidth
          sx={{ mb: 2 }}
        />
      </Box>
    );
}
