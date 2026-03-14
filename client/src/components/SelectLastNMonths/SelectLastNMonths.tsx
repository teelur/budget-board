import { Button, Group } from "@mantine/core";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ISelectLastNMonthsProps {
  monthButtons: number[];
  setSelectedMonths: React.Dispatch<React.SetStateAction<Date[]>>;
}

const SelectLastNMonths = (props: ISelectLastNMonthsProps) => {
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
            // Clear prior to adding new months to prevent duplicates.
            props.setSelectedMonths([]);
            for (let i = 0; i < months; i++) {
              props.setSelectedMonths((prev) => {
                const newMonths = [...prev];
                newMonths.push(
                  dayjs().subtract(i, "month").startOf("month").toDate(),
                );
                return newMonths;
              });
            }
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
