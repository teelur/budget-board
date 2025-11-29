import classes from "./AssetItem.module.css";
import surfaceClasses from "~/styles/Surface.module.css";

import { useSortable } from "@dnd-kit/react/sortable";
import { Button, Card, Flex, Group } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { IAssetResponse } from "~/models/asset";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCenter } from "@dnd-kit/collision";
import { GripVertical } from "lucide-react";
import AssetItemContent from "./AssetItemContent/AssetItemContent";
import EditableAssetItemContent from "./EditableAssetItemContent/EditableAssetItemContent";

interface AssetItemProps {
  asset: IAssetResponse;
  userCurrency: string;
  isSortable: boolean;
  container: Element;
  openDetails: (asset: IAssetResponse | undefined) => void;
}

const AssetItem = (props: AssetItemProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const { ref, handleRef } = useSortable({
    id: props.asset.id,
    index: props.asset.index,
    modifiers: [
      RestrictToElement.configure({
        element: props.container,
      }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCenter,
  });

  return (
    <Card
      ref={props.isSortable ? ref : undefined}
      className={`${surfaceClasses.root} ${isSelected ? "" : classes.card}`}
      shadow="sm"
      padding="0.5rem"
      radius="md"
      withBorder
      onClick={() => !isSelected && props.openDetails(props.asset)}
    >
      <Group w="100%" gap="0.5rem" wrap="nowrap">
        {props.isSortable && (
          <Flex style={{ alignSelf: "stretch" }}>
            <Button ref={handleRef} h="100%" px={0} w={30} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        {isSelected ? (
          <EditableAssetItemContent
            asset={props.asset}
            userCurrency={props.userCurrency}
            toggle={toggle}
          />
        ) : (
          <AssetItemContent
            asset={props.asset}
            userCurrency={props.userCurrency}
            toggle={toggle}
          />
        )}
      </Group>
    </Card>
  );
};

export default AssetItem;
