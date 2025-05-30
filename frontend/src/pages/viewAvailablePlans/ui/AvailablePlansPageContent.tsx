import { plansApi } from "@features/plans";
import { IGetPlanListResult } from "@features/plans/models/GetPlanList";
import { guid } from "@shared/types/guid";
import AlertDialog from "@shared/ui/AlertDialog";
import { AvailableAddressesTableWidget, SearchAddressInTableFormWidget, useSearchAvailableAddress } from "@widgets/address";
import { useEffect, useState } from "react";

export default function AvailablePlansPageContent({data}: {data: IGetPlanListResult[]}) {
    const [addresses, setAddresses] = useState(data.map(x => ({buildingId: x.buildingId, address: x.buildingAddress, buildingName: x.buildingName})));
    const { searchQuery, setSearchQuery, filteredAddresses } = useSearchAvailableAddress(addresses);
    const [requestRemoval, {error, reset}] = plansApi.useRequestPlanRemovalMutation();

    const onRemoveAddressClicked = (buildingId: guid) => {
        const indexOfRemovalAddress = addresses.findIndex(x => x.buildingId === buildingId);
        if (indexOfRemovalAddress === -1) {
            return;
        }

        const removingBuilding = addresses[indexOfRemovalAddress];
        requestRemoval({buildingId: removingBuilding.buildingId});
        setAddresses(addresses.splice(indexOfRemovalAddress, 1));
    }

    useEffect(() => {
        //todo: handler error and inform user with alert
    }, [error]);

    return <>
        <SearchAddressInTableFormWidget  searchQuery={searchQuery} setSearchQuery={setSearchQuery}/>
        <AvailableAddressesTableWidget addresses={filteredAddresses} 
            paginationModel={{
                pageSize: 5,
                page: 0
            }} 
            pageSizeOptions={[5, 10, 25]}
            onRemoveAddressClicked={onRemoveAddressClicked}
        />
        <AlertDialog 
            open={error !== undefined}
            handleClose={reset}
            title="Не удалось оставить заявку об удалении."
        />
    </>
}