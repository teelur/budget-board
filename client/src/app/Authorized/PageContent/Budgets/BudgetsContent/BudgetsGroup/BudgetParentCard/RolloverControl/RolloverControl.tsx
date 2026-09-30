import { Group } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import Checkbox from "~/components/core/Checkbox/Checkbox";
import MonthPickerInput from "~/components/core/Input/MonthPickerInput/MonthPickerInput";

interface RolloverControlProps {
  rolloverStartMonth: string | null;
  budgetMonth: Date;
  onChange: (rolloverStartMonth: string | null) => void;
  size?: "xs" | "sm";
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
    <Group gap="0.5rem" align="center" wrap="nowrap">
      <Checkbox
        checked={props.rolloverStartMonth !== null}
        onChange={(e) => handleToggle(e.target.checked)}
        label={t("roll_over_unspent")}
        size={props.size ?? "xs"}
        elevation={1}
      />
      {props.rolloverStartMonth !== null && (
        <MonthPickerInput
          value={props.rolloverStartMonth}
          onChange={(month) => props.onChange(month)}
          maxDate={latestStartMonth.format("YYYY-MM-DD")}
          locale={dayjsLocale}
          valueFormat="MMM YYYY"
          placeholder={t("rollover_start_month")}
          aria-label={t("rollover_start_month")}
          size="xs"
          styles={{ root: { maxWidth: "130px" } }}
          elevation={1}
        />
      )}
    </Group>
  );
};

export default RolloverControl;
