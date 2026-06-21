import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DeletedAssetCard from "./DeletedAssetCard";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";

const DeletedAssets = (): React.ReactNode => {
  const { t } = useTranslation();
  const assetsQuery = useAssetsQuery();

  const getDeletedAssetsContent = (): React.ReactNode => {
    if (assetsQuery.isPending) {
      return <Skeleton height={46} radius="md" />;
    }

    const deletedAssets = (assetsQuery.data ?? []).filter(
      (asset) => asset.deleted,
    );

    if (deletedAssets.length === 0) {
      return (
        <Group justify="center">
          <DimmedText size="sm">{t("no_deleted_assets")}</DimmedText>
        </Group>
      );
    }

    return deletedAssets.map((asset) => (
      <DeletedAssetCard key={asset.id} asset={asset} />
    ));
  };

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">{t("view_and_restore_deleted_assets")}</DimmedText>
      {getDeletedAssetsContent()}
    </Stack>
  );
};

export default DeletedAssets;
