import BringMapToHomeControl from "./ui/BringMapToHomeControl";
import BuildingMap from "./ui/BuildingMap";
import ITInfrastructureDescriptor, { ITInfrastructureDescriptorProps } from "./ui/ITInfrastructureDescriptor";
import LevelPickControl from "./ui/LevelPickControl";
import RoomDescriptor, { RoomDescriptorProps } from "./ui/RoomDescriptor";

export  { BuildingMap };

export * from "./lib/leafletUtilsAdditions";

export function isITInfrastructureDescriptorProps(
    props: ITInfrastructureDescriptorProps | RoomDescriptorProps
): props is ITInfrastructureDescriptorProps {
    if ("infrastructure" in props) {
        return "roomName" in props.belongsTo;
    }
    return false;
}

export type {ITInfrastructureDescriptorProps, RoomDescriptorProps};
export { ITInfrastructureDescriptor, RoomDescriptor };
export { LevelPickControl, BringMapToHomeControl };
