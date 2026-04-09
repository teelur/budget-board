import { Group, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DeletedAssetCard from "./DeletedAssetCard";

const DeletedAssets = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<any[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as any[];
      }

      return [];
    },
  });

  const deletedAssets =
    assetsQuery.data?.filter((asset) => asset.deleted) ?? [];

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">{t("view_and_restore_deleted_assets")}</DimmedText>
      {deletedAssets.length !== 0 ? (
        deletedAssets.map((asset) => (
          <DeletedAssetCard key={asset.id} asset={asset} />
        ))
      ) : (
        <Group justify="center">
          <DimmedText size="xs">{t("no_deleted_assets")}</DimmedText>
        </Group>
      )}
    </Stack>
  );
};

export default DeletedAssets;
