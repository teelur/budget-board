import { useDisclosure } from "@mantine/hooks";
import React from "react";
import {
  INetWorthWidgetCategory,
  INetWorthWidgetLine,
} from "~/models/widgetSettings";
import NetWorthLineCategoryContent from "./NetWorthLineCategoryContent/NetWorthLineCategoryContent";
import EditableNetWorthLineCategoryContent from "./EditableNetWorthLineCategoryContent/EditableNetWorthLineCategoryContent";
import { Card } from "@teelur/budget-board-ui";

export interface NetWorthLineCategoryProps {
  category: INetWorthWidgetCategory;
  lineId: string;
  currentLineName: string;
  lines: INetWorthWidgetLine[];
  settingsId: string;
}

const NetWorthLineCategory = (
  props: NetWorthLineCategoryProps,
): React.ReactNode => {
  const [isEditing, { open, close }] = useDisclosure(false);

  return (
    <Card py="0.25rem" px="0.5rem">
      {isEditing ? (
        <EditableNetWorthLineCategoryContent
          category={props.category}
          lineId={props.lineId}
          currentLineName={props.currentLineName}
          lines={props.lines}
          settingsId={props.settingsId}
          disableEdit={close}
        />
      ) : (
        <NetWorthLineCategoryContent
          category={props.category}
          enableEdit={open}
        />
      )}
    </Card>
  );
};

export default NetWorthLineCategory;
