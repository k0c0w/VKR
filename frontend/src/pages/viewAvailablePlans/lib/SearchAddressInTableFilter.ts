import { useState } from 'react';
import { AvailableAddressModel } from './AvailableAddressModel';

export const useSearchAvailableAddress = (addresses: AvailableAddressModel[] | undefined) => {
  const [searchQuery, setSearchQuery] = useState('');

  const filteredAddresses = addresses?.filter((address) =>
    address.address.toLowerCase().includes(searchQuery.toLowerCase())
  ) || [];

  return {
    searchQuery,
    setSearchQuery,
    filteredAddresses,
  };
};