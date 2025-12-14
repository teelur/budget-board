import { ActionIcon, Flex, Group } from "@mantine/core";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { INetWorthWidgetCategory } from "~/models/widgetSettings";

interface NetWorthLineCategoryContentProps {
  category: INetWorthWidgetCategory;
  enableEdit: () => void;
}

const NetWorthLineCategoryContent = (
  props: NetWorthLineCategoryContentProps
) => {
  return (
    <Flex direction={{ base: "column", xs: "row" }} justify="space-between">
      <Group gap="0.25rem" align="center">
        {props.category.type.length > 0 ? (
          <PrimaryText size="sm">{props.category.type}</PrimaryText>
        ) : (
          <DimmedText size="sm">No Type</DimmedText>
        )}
        <ChevronRightIcon size={14} />
        {props.category.subtype.length > 0 ? (
          <DimmedText size="sm">{props.category.subtype}</DimmedText>
        ) : (
          <DimmedText size="sm">No Subtype</DimmedText>
        )}
        <ActionIcon variant="transparent" size="sm" onClick={props.enableEdit}>
          <PencilIcon size={14} />
        </ActionIcon>
      </Group>

      <PrimaryText size="sm">{props.category.value}</PrimaryText>
    </Flex>
  );
};

export default NetWorthLineCategoryContent;
