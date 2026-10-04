import React from "react";
import { SimpleGrid, Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import { useTranslation } from "react-i18next";
import { EXPORT_FIELDS } from "../ExportTransactionsModal";
import { useElementSize } from "@mantine/hooks";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { Checkbox } from "@teelur/budget-board-ui";

interface FieldSelectionCardProps {
  selectedFields: string[];
  onChange: (fields: string[]) => void;
}

const FieldSelectionCard = (
  props: FieldSelectionCardProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const { ref, width } = useElementSize();

  return (
    <Card ref={ref} w="100%" elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryHeading order={5}>{t("fields")}</PrimaryHeading>
        <SimpleGrid cols={width < 300 ? 1 : 2}>
          {EXPORT_FIELDS.map((field) => (
            <Checkbox
              key={field.key}
              checked={props.selectedFields.includes(field.key)}
              onChange={(event) => {
                const isChecked = event.currentTarget.checked;
                props.onChange(
                  isChecked
                    ? [...props.selectedFields, field.key]
                    : props.selectedFields.filter(
                        (selectedField) => selectedField !== field.key,
                      ),
                );
              }}
              label={t(field.labelKey)}
            />
          ))}
        </SimpleGrid>
      </Stack>
    </Card>
  );
};

export default FieldSelectionCard;
