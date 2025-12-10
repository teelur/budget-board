import { ActionIcon, Group } from "@mantine/core";
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
    <Group justify="space-between">
      <Group gap="0.25rem" align="center">
        <PrimaryText size="sm">{props.category.type}</PrimaryText>
        <ChevronRightIcon size={14} />
        <DimmedText size="sm">{props.category.subtype}</DimmedText>
        <ActionIcon variant="transparent" size="sm" onClick={props.enableEdit}>
          <PencilIcon size={14} />
        </ActionIcon>
      </Group>

      <PrimaryText size="sm">{props.category.value}</PrimaryText>
    </Group>
  );
};

export default NetWorthLineCategoryContent;
