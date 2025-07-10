import { Skeleton, Box } from '@mui/material';

interface AddressTableSkeletonProps {
  height?: number | string; 
}
export default function AvailableAddressesTableSkeletonWidget({height}: AddressTableSkeletonProps) {
    const rows = Array.from({ length: 5 });
    
    return <Box sx={{ height:height, mx: 'auto', my: 4, width:"100%" }}>
        {/* Header skeleton */}
        <Box sx={{ display: 'flex', mb: 1 }}>
          <Skeleton variant="text" width="60%" height={40} />
          <Skeleton variant="text" width="20%" height={40} sx={{ ml: 'auto' }} />
        </Box>
        {/* Rows skeleton */}
        {rows.map((_, index) => (
          <Box key={index} sx={{ display: 'flex', py: 1, borderBottom: '1px solid rgba(224, 224, 224, 1)' }}>
            <Skeleton variant="text" width="60%" height={40} />
            <Skeleton variant="rectangular" width={100} height={36} sx={{ ml: 'auto', borderRadius: 1 }} />
          </Box>
        ))}
        {/* Pagination skeleton */}
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
          <Skeleton variant="text" width={200} height={40} />
        </Box>
    </Box>
}