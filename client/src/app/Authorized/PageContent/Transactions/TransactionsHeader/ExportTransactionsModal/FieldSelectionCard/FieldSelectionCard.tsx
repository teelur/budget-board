import React from "react";
import { SimpleGrid, Stack } from "@mantine/core";
import { Checkbox, HeadingText } from "@teelur/budget-board-ui";
import { useTranslation } from "react-i18next";
import { EXPORT_FIELDS } from "../ExportTransactionsModal";
import { useElementSize } from "@mantine/hooks";

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
    <Stack ref={ref} w="100%" gap="0.5rem">
      <HeadingText level={5}>{t("fields")}</HeadingText>
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
  );
};

export default FieldSelectionCard;
