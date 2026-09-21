import { Group } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface SelectLastNMonthsProps {
  monthButtons: number[];
  selectedMonths: Date[];
  setSelectedMonths: React.Dispatch<React.SetStateAction<Date[]>>;
  onSelectMonths?: () => void;
  size: "xs" | "compact-sm";
  showAllButton?: boolean;
  showClearButton?: boolean;
}

const SelectLastNMonths = (props: SelectLastNMonthsProps) => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();

  return (
    <Group justify="end">
      {props.monthButtons.map((months) => (
        <Button
          variant="filled"
          color="primary"
          size={props.size}
          selected={props.selectedMonths.length !== months}
          key={months}
          onClick={() => {
            const newMonths: Date[] = [];
            for (let i = 0; i < months; i++) {
              newMonths.push(
                dayjs().subtract(i, "month").startOf("month").toDate(),
              );
            }
            props.setSelectedMonths(newMonths);
            props.onSelectMonths?.();
          }}
        >
          {t("last_n_months", { count: months })}
        </Button>
      ))}
      {props.showAllButton ? (
        <Button
          variant="filled"
          color="primary"
          size={props.size}
          onClick={() => props.setSelectedMonths([])}
        >
          {t("all")}
        </Button>
      ) : props.showClearButton ? (
        <Button
          variant="filled"
          color="primary"
          size={props.size}
          disabled={props.selectedMonths.length === 0}
          onClick={() => props.setSelectedMonths([])}
        >
          {t("clear_selection")}
        </Button>
      ) : null}
    </Group>
  );
};

export default SelectLastNMonths;
