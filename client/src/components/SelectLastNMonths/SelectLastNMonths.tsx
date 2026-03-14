import { Button, Group } from "@mantine/core";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface SelectLastNMonthsProps {
  monthButtons: number[];
  setSelectedMonths: React.Dispatch<React.SetStateAction<Date[]>>;
}

const SelectLastNMonths = (props: SelectLastNMonthsProps) => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();

  return (
    <Group w="100%" justify="end">
      {props.monthButtons.map((months) => (
        <Button
          size="compact-sm"
          variant="light"
          key={months}
          onClick={() => {
            const newMonths = [];
            for (let i = 0; i < months; i++) {
              newMonths.push(
                dayjs().subtract(i, "month").startOf("month").toDate(),
              );
            }
            props.setSelectedMonths(newMonths);
          }}
        >
          {t("last_n_months", { count: months })}
        </Button>
      ))}
      <Button
        size="compact-sm"
        variant="primary"
        onClick={() => props.setSelectedMonths([])}
      >
        {t("clear_selection")}
      </Button>
    </Group>
  );
};

export default SelectLastNMonths;
