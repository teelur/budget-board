import { Group } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { DatesRangeValue } from "@mantine/dates";
import { useTranslation } from "react-i18next";
import { mantineDateFormat } from "~/helpers/datetime";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface SelectLastNMonthsRangeProps {
  monthButtons: number[];
  setDateRange: React.Dispatch<React.SetStateAction<DatesRangeValue<string>>>;
}

const SelectLastNMonthsRange = (props: SelectLastNMonthsRangeProps) => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();

  return (
    <Group w="100%" justify="end">
      {props.monthButtons.map((months) => (
        <Button
          variant="outline"
          color="primary"
          size="compact-sm"
          key={months}
          onClick={() => {
            props.setDateRange([
              dayjs().subtract(months, "month").format(mantineDateFormat),
              dayjs().format(mantineDateFormat),
            ]);
          }}
        >
          {t("last_n_months", { count: months })}
        </Button>
      ))}
    </Group>
  );
};

export default SelectLastNMonthsRange;
