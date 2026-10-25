import { Group, Stack } from "@mantine/core";
import { Button, Modal, Select } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { parseTransactionTrendsConfiguration } from "~/helpers/widgets";
import { useUpdateWidgetSettingsMutation } from "~/hooks/mutations/widgetSettings/useUpdateWidgetSettingsMutation";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { CategoryTypes, type CategoryType } from "~/models/category";
import CategoryMultiSelect from "~/components/CategoryMultiSelect/CategoryMultiSelect";

interface TransactionTrendsWidgetSettingsProps {
  widget: IWidgetSettingsResponse;
  opened: boolean;
  onClose: () => void;
}

const TransactionTrendsWidgetSettings = ({
  widget,
  opened,
  onClose,
}: TransactionTrendsWidgetSettingsProps): React.ReactNode => {
  const { t } = useTranslation();
  const updateWidgetSettingsMutation = useUpdateWidgetSettingsMutation();
  const categoryValuesField = useField<string[]>({ initialValue: [] });
  const transactionTypeField = useField<CategoryType>({
    initialValue: CategoryTypes.Expense,
  });
  const [initialized, setInitialized] = React.useState(false);

  const handleClose = () => {
    setInitialized(false);
    categoryValuesField.reset();
    transactionTypeField.reset();
    onClose();
  };

  React.useEffect(() => {
    if (!opened || initialized) {
      return;
    }

    const configuration = parseTransactionTrendsConfiguration(
      widget.configuration,
    );
    categoryValuesField.setValue(configuration.categoryValues);
    transactionTypeField.setValue(configuration.transactionType);
    setInitialized(true);
  }, [
    categoryValuesField,
    initialized,
    opened,
    transactionTypeField,
    widget.configuration,
  ]);

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title={t("transaction_trends_widget_settings")}
      size="sm"
    >
      <Stack gap="0.75rem">
        <CategoryMultiSelect
          label={t("categories")}
          description={t("transaction_trends_widget_settings_message")}
          {...categoryValuesField.getInputProps()}
        />
        {categoryValuesField.getValue().length === 0 && (
          <Select
            label={t("transaction_type")}
            data={[
              { value: CategoryTypes.Expense, label: t("spending") },
              { value: CategoryTypes.Income, label: t("income") },
            ]}
            value={transactionTypeField.getValue()}
            onChange={(value) => {
              if (
                value === CategoryTypes.Expense ||
                value === CategoryTypes.Income
              ) {
                transactionTypeField.setValue(value);
              }
            }}
          />
        )}
        <Group gap="0.5rem">
          <Button
            variant="filled"
            color="neutral"
            size="compact-sm"
            flex="1 1 0"
            onClick={handleClose}
          >
            {t("cancel")}
          </Button>
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            flex="1 1 0"
            onClick={() => {
              updateWidgetSettingsMutation.mutate([
                {
                  id: widget.id,
                  configuration: {
                    categoryValues: categoryValuesField.getValue(),
                    transactionType: transactionTypeField.getValue(),
                  },
                },
              ]);
            }}
            loading={updateWidgetSettingsMutation.isPending}
          >
            {t("save")}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default TransactionTrendsWidgetSettings;
