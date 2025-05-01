import { Box, IconButton, } from "@mui/material";
import CloseIcon from '@mui/icons-material/Close';
import { isITInfrastructureDescriptorProps, ITInfrastructureDescriptor, RoomDescriptor, RoomDescriptorProps } from "@shared/map";
import { ITInfrastructureDescriptorProps } from "@shared/map/ui/ITInfrastructureDescriptor";
import { useMap } from "react-leaflet";
import { useCallback, useEffect, useRef } from "react";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { focusOnFeature, updateMetaProperties } from "../lib/createNewPlanSlice";
import { isRoom, ITInfrastructure, ITInfrastructureMetaProperties, Room, RoomMetaProperties } from "@entities/map";
import React from "react";
import * as turf from "@turf/turf";
import { GeoJSON } from "leaflet";

export type MapObjectDescriptionPopupContentProps = ITInfrastructureDescriptorProps | RoomDescriptorProps;

interface ContentProps {
    focusedOnFeature:{
        feature: Room | ITInfrastructure;
        additions: {
            level: {
                index: number;
                name: string;
            };
            roomName: string;
        };
    };
    onClose: () => void;
}
const Content = React.forwardRef<HTMLDivElement, ContentProps>(function Content({onClose, focusedOnFeature}:  ContentProps, ref) {
    const dispatch = useAppDispatch();
    
    const handlePopupClick = (e: React.MouseEvent) => {
        e.stopPropagation();
    };

    const {feature, additions} = focusedOnFeature!;
    const {level, roomName} = additions;

    let popupContentProps: ITInfrastructureDescriptorProps | RoomDescriptorProps;
    if (isRoom(feature)) {
        popupContentProps = {
            room: feature,
            belongsTo: {
                levelName: level.name
            },
            onMetaPropsChange: (roomUpdate:RoomMetaProperties) => dispatch(updateMetaProperties({levelIndex:level.index, featureId: feature.id, props:roomUpdate}))
        }
    } else {
        popupContentProps = {
            infrastructure: feature,
            belongsTo: {
                levelName: level.name,
                roomName: roomName
            },
            onMetaPropsChange: (infr: ITInfrastructureMetaProperties) => dispatch(updateMetaProperties({levelIndex:level.index, featureId: feature.id, props:infr}))
        }
    }

    return <Box
        ref={ref}
        onClick={handlePopupClick}
        sx={{
            position: "absolute",
            top: "80px", // Below any top controls (e.g., level selector)
            right: "10px",
            width: "250px",
            border: "1px solid #ccc",
            borderRadius: "8px",
            padding: "10px",
            backgroundColor: "white",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
            zIndex: 1000, // Ensure it overlays the map
        }}
    >
        <Box display="flex" justifyContent="flex-end">
            <IconButton onClick={onClose} size="small">
                <CloseIcon />
            </IconButton>
        </Box>
        { isITInfrastructureDescriptorProps(popupContentProps) ? <ITInfrastructureDescriptor {...popupContentProps} /> : <RoomDescriptor {...popupContentProps} />}
    </Box>
})

export default function MapObjectDescriptionPopup() {
    const focusedOnFeature = useAppSelector(state => state.createNewPlanReducer.selectedFeatureInfo);
    const popupRef = useRef<HTMLDivElement>(null); 
    const dispatch = useAppDispatch();
    const map = useMap();

    const onClose = useCallback(()=> {
        dispatch(focusOnFeature());
    }, [dispatch]);

    useEffect(() => {
        function handleMapClick(e: L.LeafletMouseEvent) {
            if (focusedOnFeature === undefined) {
                return;
            }
            const target = e.originalEvent.target as Node;
            const clickWasInsideThePopup = popupRef.current && popupRef.current.contains(target);
            if (clickWasInsideThePopup) {
                return;
            }
            const clickWasInsideFeature = turf.booleanContains(focusedOnFeature.feature, turf.point(GeoJSON.latLngToCoords(e.latlng)));
            if (clickWasInsideFeature) {
                return;
            }

            onClose();
        }

        map.on('click', handleMapClick);
        map.on('levelpicker:changelevel', onClose);

        return () => {
            map.off('click', handleMapClick);
            map.off('levelpicker:changelevel', onClose);
        }
    }, [map, focusedOnFeature, onClose]);

    return <>{focusedOnFeature !== undefined && <Content ref={popupRef} onClose={onClose} focusedOnFeature={focusedOnFeature} />}</>
}
