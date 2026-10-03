import React from "react";
import { Group, Stack } from "@mantine/core";
import { Button, NumberInput, SegmentedControl } from "@teelur/budget-board-ui";
import { MoveLeftIcon } from "lucide-react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTranslation } from "react-i18next";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { mantineDateFormat } from "~/helpers/datetime";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { DateInput } from "@mantine/dates";

interface SetTargetProps {
  goBackToPreviousDialog: () => void;
  createGoal: (completeDate: Date | null, monthlyContribution: number) => void;
  isCreatingGoal: boolean;
}

const SetTarget = (props: SetTargetProps): React.ReactNode => {
  const [targetType, setTargetType] = React.useState<
    "completeDate" | "monthlyContribution"
  >("completeDate");
  const goalCompleteDateField = useField<Date | null>({
    initialValue: null,
  });
  const goalMonthlyContributionField = useField<number>({
    initialValue: 0,
  });

  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    currencySymbol,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { decimalPlaces } = useUserSettings();

  useDidUpdate(() => {
    goalCompleteDateField.reset();
    goalMonthlyContributionField.reset();
  }, [targetType]);

  const isTargetValid =
    targetType === "completeDate"
      ? goalCompleteDateField.getValue() !== null
      : goalMonthlyContributionField.getValue() > 0;

  return (
    <Stack gap={"1rem"}>
      <SegmentedControl
        color="secondary"
        size="compact-sm"
        fullWidth
        value={targetType}
        data={[
          { label: t("complete_date"), value: "completeDate" },
          { label: t("monthly_contribution"), value: "monthlyContribution" },
        ]}
        onChange={(value) =>
          setTargetType(value as "completeDate" | "monthlyContribution")
        }
        aria-label={t("target_type")}
      />
      {targetType === "completeDate" && (
        <DateInput
          label={t("complete_date")}
          placeholder={t("select_a_completion_date")}
          clearable
          {...goalCompleteDateField.getInputProps()}
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          minDate={dayjs().format(mantineDateFormat)}
        />
      )}
      {targetType === "monthlyContribution" && (
        <NumberInput
          label={t("monthly_contribution")}
          placeholder={t("enter_monthly_contribution")}
          prefix={currencySymbol}
          min={0}
          decimalScale={decimalPlaces}
          thousandSeparator={thousandsSeparator}
          decimalSeparator={decimalSeparator}
          {...goalMonthlyContributionField.getInputProps()}
        />
      )}
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="sm"
          flex="1 1 0"
          onClick={() => props.goBackToPreviousDialog()}
        >
          {<MoveLeftIcon size={16} />}
        </Button>
        <Button
          variant="filled"
          color="primary"
          size="sm"
          flex="1 1 0"
          onClick={() => {
            props.createGoal(
              goalCompleteDateField.getValue(),
              goalMonthlyContributionField.getValue(),
            );
          }}
          disabled={!isTargetValid || props.isCreatingGoal}
          loading={props.isCreatingGoal}
        >
          {t("create_goal")}
        </Button>
      </Group>
    </Stack>
  );
};

export default SetTarget;
