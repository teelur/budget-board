import React from "react";
import { Button, Group, SegmentedControl, Stack } from "@mantine/core";
import { MoveLeftIcon } from "lucide-react";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { mantineDateFormat } from "~/helpers/datetime";

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
        value={targetType}
        onChange={(value) =>
          setTargetType(value as "completeDate" | "monthlyContribution")
        }
        data={[
          { label: t("complete_date"), value: "completeDate" },
          { label: t("monthly_contribution"), value: "monthlyContribution" },
        ]}
        radius="sm"
        color={"indigo"}
      />
      {targetType === "completeDate" && (
        <DateInput
          label={<PrimaryText size="sm">{t("complete_date")}</PrimaryText>}
          placeholder={t("select_a_completion_date")}
          clearable
          {...goalCompleteDateField.getInputProps()}
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          minDate={dayjs().format(mantineDateFormat)}
          elevation={1}
        />
      )}
      {targetType === "monthlyContribution" && (
        <NumberInput
          label={
            <PrimaryText size="sm">{t("monthly_contribution")}</PrimaryText>
          }
          placeholder={t("enter_monthly_contribution")}
          prefix={currencySymbol}
          min={0}
          decimalScale={2}
          thousandSeparator={thousandsSeparator}
          decimalSeparator={decimalSeparator}
          {...goalMonthlyContributionField.getInputProps()}
          elevation={1}
        />
      )}
      <Group w="100%">
        <Button flex="1 1 0" onClick={() => props.goBackToPreviousDialog()}>
          {<MoveLeftIcon size={16} />}
        </Button>
        <Button
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
