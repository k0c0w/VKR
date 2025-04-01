import { Address } from "@shared/types/ValueObjectsTypes";
import { useState } from "react";
import AddressForm from "./AddressForm";
import { Container } from "@mui/material";
import AddressLoader from "./AddressLoader";
import { Building } from "@entities/map/Building";

interface LoadBuildingWidgetProps {
    setBuilding: (building: Building) => void;
}

export default function LoadBuildingWidget({setBuilding}:LoadBuildingWidgetProps) {
    const [address, setAddress] = useState<Address | undefined>();

    return <Container component="main" maxWidth="tablet">
        {!address && <AddressForm onSubmit={setAddress}/>}
        {address && <AddressLoader 
            address={address} 
            setBuildingInfo={({bounds, levels}) => setBuilding({address, boundaries: bounds, levels})} 
        />}
    </Container>
}