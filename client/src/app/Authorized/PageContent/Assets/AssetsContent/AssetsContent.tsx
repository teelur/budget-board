import { Group, LoadingOverlay, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { IAssetIndexRequest, IAssetResponse } from "~/models/asset";
import AssetItem from "./AssetItem/AssetItem";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import AssetDetails from "./AssetDetails/AssetDetails";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { InfoIcon } from "lucide-react";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";
import { useOrderAssetsMutation } from "~/hooks/mutations/assets/useOrderAssetsMutation";

interface AssetsContentProps {
  isSortable: boolean;
}

const AssetsContent = (props: AssetsContentProps): React.ReactNode => {
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure(false);
  const [selectedAsset, setSelectedAsset] = React.useState<
    IAssetResponse | undefined
  >(undefined);

  const { t } = useTranslation();
  const assetsQuery = useAssetsQuery();

  const [sortedAssets, setSortedAssets] = React.useState<IAssetResponse[]>([]);

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
          })),
      );
    }
  }, [assetsQuery.data]);

  const orderAssetsMutation = useOrderAssetsMutation();

  useDidUpdate(() => {
    if (!props.isSortable) {
      const indexedAssets: IAssetIndexRequest[] = sortedAssets.map(
        (asset, index) => ({
          id: asset.id,
          index,
        }),
      );
      orderAssetsMutation.mutate(indexedAssets);
    }
  }, [props.isSortable]);

  return (
    <Stack id="assets-stack" gap="1rem">
      <LoadingOverlay visible={orderAssetsMutation.isPending} />
      <AssetDetails
        isOpen={isDetailsOpen}
        close={closeDetails}
        asset={selectedAsset}
      />
      {assetsQuery.isPending ? (
        <>
          <Skeleton height={60} radius="md" />
          <Skeleton height={60} radius="md" />
          <Skeleton height={60} radius="md" />
        </>
      ) : sortedAssets.length === 0 ? (
        <Group justify="center" align="center" gap="0.5rem">
          <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
          <DimmedText size="sm">{t("no_assets")}</DimmedText>
        </Group>
      ) : (
        <DragDropProvider
          onDragEnd={(event) => {
            const updatedList = move(sortedAssets, event).map(
              (asset, index) =>
                ({
                  ...asset,
                  index,
                }) as IAssetResponse,
            );

            setSortedAssets(updatedList);
          }}
        >
          {sortedAssets.map((asset) => (
            <AssetItem
              key={asset.id}
              asset={asset}
              isSortable={props.isSortable}
              container={document.getElementById("assets-stack") as Element}
              openDetails={function (asset: IAssetResponse | undefined): void {
                setSelectedAsset(asset);
                openDetails();
              }}
            />
          ))}
        </DragDropProvider>
      )}
    </Stack>
  );
};

export default AssetsContent;
