import React from "react";
import { Stack } from "@mantine/core";
import { HeadingText } from "@teelur/budget-board-ui";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { useTranslation } from "react-i18next";
import ColumnOrderItem from "./ColumnOrderItem/ColumnOrderItem";
import { EXPORT_FIELDS } from "../ExportTransactionsModal";

interface ColumnOrderCardProps {
  orderedFields: string[];
  onChange: (fields: string[]) => void;
}

const ColumnOrderCard = (props: ColumnOrderCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const [listElement, setListElement] = React.useState<HTMLDivElement | null>(
    null,
  );

  const fieldLabelMap = React.useMemo(
    () => Object.fromEntries(EXPORT_FIELDS.map((f) => [f.key, t(f.labelKey)])),
    [t],
  );

  return (
    <Stack w="100%" gap="0.5rem">
      <HeadingText level={5}>{t("column_order")}</HeadingText>
      <DragDropProvider
        onDragEnd={(event) => {
          props.onChange(move(props.orderedFields, event));
        }}
      >
        <Stack ref={setListElement} gap="0.5rem">
          {props.orderedFields.map((key, index) => (
            <ColumnOrderItem
              key={key}
              fieldKey={key}
              label={fieldLabelMap[key] ?? key}
              index={index}
              container={listElement ?? undefined}
            />
          ))}
        </Stack>
      </DragDropProvider>
    </Stack>
  );
};

export default ColumnOrderCard;
