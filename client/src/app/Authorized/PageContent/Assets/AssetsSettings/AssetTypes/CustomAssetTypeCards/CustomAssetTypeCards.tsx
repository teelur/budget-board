import { Group, Stack } from "@mantine/core";
import { useTranslation } from "react-i18next";
import React from "react";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { IAssetTypeResponse } from "~/models/assetType";
import { defaultGuid } from "~/models/applicationUser";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import CustomAssetTypeCard from "./CustomAssetTypeCard/CustomAssetTypeCard";

const CustomAssetTypeCards = (): React.ReactNode => {
  const { t } = useTranslation();
  const { allAssetTypes, customAssetTypes } = useAssetTypes();

  if (customAssetTypes.length === 0) {
    return (
      <Group justify="center" p="1rem">
        <DimmedText size="sm">{t("no_custom_asset_types")}</DimmedText>
      </Group>
    );
  }

  const customParents = customAssetTypes.filter((type) => type.parent === "");
  const customChildren = customAssetTypes.filter((type) => type.parent !== "");

  const childrenByParent = new Map<string, IAssetTypeResponse[]>();
  for (const child of customChildren) {
    const key = child.parent.toLowerCase();
    if (!childrenByParent.has(key)) childrenByParent.set(key, []);
    childrenByParent.get(key)!.push(child);
  }

  const customParentKeys = new Set(
    customParents.map((t) => t.value.toLowerCase()),
  );
  const builtInParentsNeeded = new Map<string, IAssetTypeResponse>();
  for (const child of customChildren) {
    const key = child.parent.toLowerCase();
    if (!customParentKeys.has(key)) {
      const builtIn = allAssetTypes.find(
        (t) => t.id === defaultGuid && t.value.toLowerCase() === key,
      );
      if (builtIn && !builtInParentsNeeded.has(key)) {
        builtInParentsNeeded.set(key, builtIn);
      }
    }
  }

  type TypeGroup = {
    parent: IAssetTypeResponse;
    isBuiltIn: boolean;
    children: IAssetTypeResponse[];
  };

  const groups: TypeGroup[] = [
    ...customParents.map((p) => ({
      parent: p,
      isBuiltIn: false,
      children: childrenByParent.get(p.value.toLowerCase()) ?? [],
    })),
    ...[...builtInParentsNeeded.values()].map((p) => ({
      parent: p,
      isBuiltIn: true,
      children: childrenByParent.get(p.value.toLowerCase()) ?? [],
    })),
  ].sort((a, b) => a.parent.value.localeCompare(b.parent.value));

  return (
    <Stack gap="0.5rem">
      {groups.map((group) => (
        <Stack key={group.parent.value} align="flex-start" gap="0.5rem">
          <CustomAssetTypeCard
            assetType={group.parent}
            isBuiltIn={group.isBuiltIn}
          />
          {group.children.map((child) => (
            <CustomAssetTypeCard key={child.id} assetType={child} isChildCard />
          ))}
        </Stack>
      ))}
    </Stack>
  );
};

export default CustomAssetTypeCards;
