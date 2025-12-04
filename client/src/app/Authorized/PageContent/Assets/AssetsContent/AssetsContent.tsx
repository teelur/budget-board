import { LoadingOverlay, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IAssetIndexRequest, IAssetResponse } from "~/models/asset";
import AssetItem from "./AssetItem/AssetItem";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { AxiosError } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import AssetDetails from "./AssetDetails/AssetDetails";

interface AssetsContentProps {
  isSortable: boolean;
}

const AssetsContent = (props: AssetsContentProps): React.ReactNode => {
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure(false);
  const [selectedAsset, setSelectedAsset] = React.useState<
    IAssetResponse | undefined
  >(undefined);

  const [sortedAssets, setSortedAssets] = React.useState<IAssetResponse[]>([]);

  const { request } = useAuth();
  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async () => {
      const res = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetResponse[];
      }

      return undefined;
    },
  });

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async () => {
      const res = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data;
      }

      return undefined;
    },
  });

  React.useEffect(() => {
    if (assetsQuery.data) {
      // Some assets might have conflicting indices, so we need to re-index them here
      // to ensure the drag-and-drop functionality works correctly
      setSortedAssets(
        assetsQuery.data
          .slice()
          .filter((asset) => !asset.deleted)
          .sort((a, b) => a.index - b.index)
          .map((asset, index) => ({
            ...asset,
            index,
          }))
      );
    }
  }, [assetsQuery.data]);

  const queryClient = useQueryClient();
  const doIndexAssets = useMutation({
    mutationFn: async (assets: IAssetIndexRequest[]) =>
      await request({
        url: "/api/asset/order",
        method: "PUT",
        data: assets,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["assets"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  useDidUpdate(() => {
    if (!props.isSortable) {
      const indexedAssets: IAssetIndexRequest[] = sortedAssets.map(
        (asset, index) => ({
          id: asset.id,
          index,
        })
      );
      doIndexAssets.mutate(indexedAssets);
    }
  }, [props.isSortable]);

  return (
    <Stack id="assets-stack" gap="1rem">
      <LoadingOverlay visible={doIndexAssets.isPending} />
      <AssetDetails
        isOpen={isDetailsOpen}
        close={closeDetails}
        asset={selectedAsset}
        userCurrency={userSettingsQuery.data?.currency || "USD"}
      />
      <DragDropProvider
        onDragEnd={(event) => {
          const updatedList = move(sortedAssets, event).map(
            (asset, index) =>
              ({
                ...asset,
                index,
              } as IAssetResponse)
          );

          setSortedAssets(updatedList);
        }}
      >
        {sortedAssets.map((asset) => (
          <AssetItem
            key={asset.id}
            asset={asset}
            userCurrency={userSettingsQuery.data?.currency || "USD"}
            isSortable={props.isSortable}
            container={document.getElementById("assets-stack") as Element}
            openDetails={function (asset: IAssetResponse | undefined): void {
              setSelectedAsset(asset);
              openDetails();
            }}
          />
        ))}
      </DragDropProvider>
    </Stack>
  );
};

export default AssetsContent;
