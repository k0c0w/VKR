import { Box, Grid, Skeleton } from "@mui/material";
import { AvailableAddressesTableSkeletonWidget } from "@widgets/address";

export function AvailablePlansPageContentSkeleton() {
  return (
    <Box sx={{ width: '100%', minHeight: '400px' }}>
      {/* Region Selector and Search Input Skeleton */}
      <Grid
        container
        spacing={2}
        sx={{ mb: 3, flexWrap: { mobile: 'wrap', tablet: 'nowrap' } }}
      >
        <Grid size={{ mobile: 12, tablet: 3 }} component="div">
          <Skeleton variant="rectangular" height={56} />
        </Grid>
        <Grid size={{ mobile: 12, tablet: 7 }} component="div">
          <Skeleton variant="rectangular" height={56} />
        </Grid>
      </Grid>

      {/* Table Skeleton */}
      <AvailableAddressesTableSkeletonWidget height={400} />
    </Box>
  );
}