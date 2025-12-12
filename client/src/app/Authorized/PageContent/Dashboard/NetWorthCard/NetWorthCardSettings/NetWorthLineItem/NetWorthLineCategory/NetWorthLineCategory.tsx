import { useDisclosure } from "@mantine/hooks";
import React from "react";
import Card from "~/components/core/Card/Card";
import { INetWorthWidgetCategory } from "~/models/widgetSettings";
import NetWorthLineCategoryContent from "./NetWorthLineCategoryContent/NetWorthLineCategoryContent";
import EditableNetWorthLineCategoryContent from "./EditableNetWorthLineCategoryContent/EditableNetWorthLineCategoryContent";

export interface NetWorthLineCategoryProps {
  category: INetWorthWidgetCategory;
  lineId: string;
  currentLineName: string;
}

const NetWorthLineCategory = (
  props: NetWorthLineCategoryProps
): React.ReactNode => {
  const [isEditing, { open, close }] = useDisclosure(false);

  return (
    <Card elevation={2}>
      {isEditing ? (
        <EditableNetWorthLineCategoryContent
          category={props.category}
          lineId={props.lineId}
          currentLineName={props.currentLineName}
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
