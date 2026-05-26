import React from "react";
import { Stack } from "@mantine/core";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import ColumnOrderItem from "./ColumnOrderItem/ColumnOrderItem";
import { EXPORT_FIELDS } from "../ExportTransactionsModal";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";

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
    <Card w="100%" elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryHeading order={5}>{t("column_order")}</PrimaryHeading>
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
    </Card>
  );
};

export default ColumnOrderCard;
