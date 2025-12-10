import { useDisclosure } from "@mantine/hooks";
import React from "react";
import Card from "~/components/core/Card/Card";
import { INetWorthWidgetCategory } from "~/models/widgetSettings";
import NetWorthLineCategoryContent from "./NetWorthLineCategoryContent/NetWorthLineCategoryContent";
import EditableNetWorthLineCategoryContent from "./EditableNetWorthLineCategoryContent/EditableNetWorthLineCategoryContent";

export interface NetWorthLineCategoryProps {
  category: INetWorthWidgetCategory;
  index: number;
  currentLineName: string;
  updateNetWorthCategory: (
    updatedCategory: INetWorthWidgetCategory,
    index: number
  ) => void;
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
          index={props.index}
          currentLineName={props.currentLineName}
          updateNetWorthCategory={props.updateNetWorthCategory}
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
