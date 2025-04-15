import { Address } from "@shared/types/ValueObjectsTypes";
import { ReactNode, useState } from "react";
import AddressForm from "./AddressForm";
import { Container } from "@mui/material";
import AddressLoader from "./AddressLoader";
import { Building } from "@entities/map/Building";

interface LoadBuildingWidgetProps {
    setBuilding: (building: Building) => void;
    loaderBackground?: ReactNode
}

export default function LoadBuildingWidget({setBuilding, loaderBackground}:LoadBuildingWidgetProps) {
    const [address, setAddress] = useState<Address | undefined>();

    return <Container maxWidth="tablet">
        {!address && <AddressForm onSubmit={setAddress}/>}
        {address && <>
                <AddressLoader 
                    address={address} 
                    setBuilding={setBuilding} 
                />
                {loaderBackground}
            </>
        }
    </Container>
}