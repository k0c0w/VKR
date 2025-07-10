import React from 'react';
import { FormControl, OutlinedInput } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';

interface SearchProps {
  placeHolder: string;
  searchQuery: string;
  setSearchQuery: (query: string) => void;
}

export default function SearchQueryFilterInput ({placeHolder,  searchQuery, setSearchQuery }: SearchProps) {
    const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => setSearchQuery(event.target.value);

    return (
    <FormControl fullWidth>
        <OutlinedInput
            placeholder={placeHolder}
            value={searchQuery}
            onChange={handleSearchChange}
            startAdornment={<SearchIcon fontSize="small" opacity={0.7} />}
        />
    </FormControl>
    );
}
