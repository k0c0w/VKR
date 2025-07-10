import { useState } from 'react';
import { User } from '@entities/user';

export const useFilterUsersByEmail = (users: User[] | undefined) => {
  const [searchQuery, setSearchQuery] = useState('');

  const filteredUsers = users?.filter((user) =>
    user.email.toLowerCase().includes(searchQuery.toLowerCase())
  ) || [];

  return {
    searchQuery,
    setSearchQuery,
    filteredUsers,
  };
};