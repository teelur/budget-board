import React from "react";
import { Checkbox, SimpleGrid, Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
    <Card ref={ref} w="100%" elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryText size="lg">{t("fields")}</PrimaryText>
        <Checkbox.Group value={props.selectedFields} onChange={props.onChange}>
          <SimpleGrid cols={width < 300 ? 1 : 2}>
            {EXPORT_FIELDS.map((field) => (
              <Checkbox
                key={field.key}
                value={field.key}
                label={<PrimaryText size="sm">{t(field.labelKey)}</PrimaryText>}
              />
            ))}
          </SimpleGrid>
        </Checkbox.Group>
      </Stack>
    </Card>
  );
};

export default FieldSelectionCard;
