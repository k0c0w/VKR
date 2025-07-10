import { AddressFromCatalogueFormWidget } from "@widgets/address";
import { ReactNode } from "react";

interface SelectBuildingFromCatalogueProps {
  onSelectBuilding: (data: { region: string; name: string; address: string }) => void;
  onError: (error: {title: string; payload?: ReactNode } | undefined) => void;
}

export function SelectBuildingFromCatalogue({
  onSelectBuilding,
  onError,
}: SelectBuildingFromCatalogueProps) {

  const handleSubmit = (data: { region: string; name: string; address: string }) => {
    if (!data.region || !data.name || !data.address) {
      onError({
        title: "Ошибка при выборе здания",
        payload: "Пожалуйста, заполните все поля: регион, название и адрес",
      });
      return;
    }
    onSelectBuilding(data);
  };

  return <AddressFromCatalogueFormWidget onSubmit={handleSubmit} />
}