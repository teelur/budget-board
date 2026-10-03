import { Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import MonthPickerInput from "~/components/core/Input/MonthPickerInput/MonthPickerInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface RolloverControlProps {
  rolloverStartMonth: string | null;
  budgetMonth: Date;
  onChange: (rolloverStartMonth: string | null) => void;
  size?: "xs" | "sm";
  /** Why rollover can't be turned on here, e.g. because a related category already rolls over. */
  disabledReason?: string;
}

const RolloverControl = (props: RolloverControlProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dayjsLocale } = useLocale();

  // Rollover accumulates from prior months, so it can never start on the budget's own month.
  const latestStartMonth = dayjs(props.budgetMonth)
    .startOf("month")
    .subtract(1, "month");

  const handleToggle = (checked: boolean) =>
    props.onChange(checked ? latestStartMonth.format("YYYY-MM-DD") : null);

  return (
    <Stack gap="0.75rem">
      <Checkbox
        checked={props.rolloverStartMonth !== null}
        onChange={(e) => handleToggle(e.target.checked)}
        label={t("roll_over_unspent")}
        disabled={
          props.disabledReason !== undefined &&
          props.rolloverStartMonth === null
        }
        description={
          props.rolloverStartMonth === null ? props.disabledReason : undefined
        }
        styles={{
          description: { color: "var(--text-color-status-warning)" },
        }}
        size={props.size ?? "xs"}
        elevation={1}
      />
      {props.rolloverStartMonth !== null && (
        <Group gap="0.5rem" align="center" wrap="nowrap">
          <PrimaryText size="sm">
            {t("roll_over_money_since")}
          </PrimaryText>
          <MonthPickerInput
            value={props.rolloverStartMonth}
            onChange={(month) => props.onChange(month)}
            maxDate={latestStartMonth.format("YYYY-MM-DD")}
            locale={dayjsLocale}
            valueFormat="MMM YYYY"
            placeholder={t("rollover_start_month")}
            aria-label={t("roll_over_money_since")}
            size="xs"
            styles={{ root: { maxWidth: "130px" } }}
            elevation={1}
          />
        </Group>
      )}
    </Stack>
  );
};

export default RolloverControl;
