import { CatalogueItEquipment } from "@entities/catalogue";

export interface ItEquipmetCatalogue {
    [roomId: number]: CatalogueItEquipment[];
}