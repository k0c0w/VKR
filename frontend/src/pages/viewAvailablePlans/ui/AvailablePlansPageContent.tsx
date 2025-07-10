import { useEffect, useState } from "react";
import { Box, Grid, Select, MenuItem } from "@mui/material";
import { AvailableAddressesTableWidget } from "@widgets/address";
import { useSearchAvailableAddress } from "../lib/SearchAddressInTableFilter";
import guid from "@shared/types/guid";
import { plansApi } from "@features/plans";
import { IGetPlanListResult } from "@features/plans/models/GetPlanList";
import AlertDialog from "@shared/ui/AlertDialog";
import SearchQueryFilterInput from "@shared/ui/SearchQueryFilterInput";
import { AvailableAddressModel } from "../lib/AvailableAddressModel";

export default function AvailablePlansPageContent({ data }: { data: IGetPlanListResult[] }) {
  const groupedByRegions = data.reduce<{ [region: string]: AvailableAddressModel[] }>((acc, val) => {
    const model: AvailableAddressModel = {
      buildingId: val.planId,
      address: val.address,
      buildingName: val.buildingName,
    };
    if (!acc[val.region]) {
      acc[val.region] = [];
    }
    acc[val.region].push(model);
    return acc;
  }, {});

  const regions = Object.keys(groupedByRegions);
  const [selectedRegion, setSelectedRegion] = useState(regions.length === 0 ? '' : regions[0]);
  const [addresses, setAddresses] = useState(groupedByRegions[selectedRegion] ?? []);
  const { searchQuery, setSearchQuery, filteredAddresses } = useSearchAvailableAddress(addresses);
  const [requestRemoval, { error, reset }] = plansApi.useRequestPlanRemovalMutation();

  const onRemoveAddressClicked = (buildingId: guid) => {
    const indexOfRemovalAddress = addresses.findIndex((x) => x.buildingId === buildingId);
    if (indexOfRemovalAddress === -1) {
      return;
    }
    const removingBuilding = addresses[indexOfRemovalAddress];
    requestRemoval({ buildingId: removingBuilding.buildingId });
    // Create a new array instead of mutating the state
    setAddresses(addresses => [...addresses.slice(0, indexOfRemovalAddress), ...addresses.slice(indexOfRemovalAddress + 1)]);
  };

  useEffect(() => {
    setAddresses(groupedByRegions[selectedRegion] ?? []);
  }, [selectedRegion]);

  return (
    <Box sx={{ width: '100%', minHeight: '400px' }}>
      {/* Region Selector and Search Input */}
      {regions.length > 0 && 
        <Grid
          container
          spacing={2}
          sx={{ mb: 3, flexWrap: { mobile: 'wrap', tablet: 'nowrap' } }}
        >
          <Grid size={{ mobile: 12, tablet: 3, lg: 3, xl: 3 }} component="div">
            <Select
              fullWidth
              value={selectedRegion}
              onChange={(e) => setSelectedRegion(e.target.value)}
              displayEmpty
              variant="outlined"
            >
              {Object.keys(groupedByRegions).map((region) => (
                <MenuItem key={region} value={region}>
                  {region}
                </MenuItem>
              ))}
            </Select>
          </Grid>
          <Grid size={{ mobile: 12, tablet: 9, lg: 9, xl: 9 }} component="div">
            <SearchQueryFilterInput
              placeHolder="Фильтр"
              searchQuery={searchQuery} 
              setSearchQuery={setSearchQuery}
            />
          </Grid>
        </Grid>
      }

      {/* Plans Table */}
      <AvailableAddressesTableWidget
        addresses={filteredAddresses}
        paginationModel={{
          pageSize: 5,
          page: 0,
        }}
        pageSizeOptions={[5, 10, 25]}
        onRemoveAddressClicked={onRemoveAddressClicked}
      />

      {/* Alert Dialog */}
      <AlertDialog
        open={error !== undefined}
        handleClose={reset}
        title="Не удалось удалить план."
      />
    </Box>
  );
}