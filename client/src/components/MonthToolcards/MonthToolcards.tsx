import { ActionIcon, Group } from "@mantine/core";
import { useElementSize } from "@mantine/hooks";
import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import React from "react";
import MonthToolcard from "./MonthToolcard/MonthToolcard";
import { getCashFlowValue } from "~/helpers/budgets";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface MonthToolcardsProps {
  selectedDates: Date[];
  setSelectedDates: React.Dispatch<React.SetStateAction<Date[]>>;
  timeToMonthlyTotalsMap: Map<number, number>;
  isPending: boolean;
  allowSelectMultiple: boolean;
  allowFutureMonths?: boolean;
}

const MonthToolcards = (props: MonthToolcardsProps): React.ReactNode => {
  const PAGE_BUTTON_WIDTH = 36;
  const MONTH_CARD_WIDTH = 68;

  const { dayjs } = useDate();

  const [index, setIndex] = React.useState(0);
  const { ref, width } = useElementSize();

  const dates = Array.from(
    {
      length: Math.floor(
        ((width ?? 0) - PAGE_BUTTON_WIDTH * 2) / MONTH_CARD_WIDTH,
      ),
    },
    (_, i) =>
      dayjs()
        .subtract(i + index, "month")
        .startOf("month")
        .toDate(),
  );

  const handleClick = (date: Date) => {
    if (props.allowSelectMultiple) {
      // When select multiple is on, we need to add/remove selected dates when clicked.
      if (
        props.selectedDates.find((item) => item.getTime() === date.getTime())
      ) {
        props.setSelectedDates(
          props.selectedDates.filter(
            (selectedDate: Date) => selectedDate.getTime() !== date.getTime(),
          ),
        );
      } else {
        // If it isn't present, then add to selected.
        props.setSelectedDates([date, ...props.selectedDates]);
      }
    } else {
      // When select multiple is off, we should switch the date when clicked.
      props.setSelectedDates([date]);
    }
  };

  return (
    <Group wrap="nowrap" gap="0.5rem" ref={ref}>
      <ActionIcon
        h="62px"
        w="32px"
        flex="0 0"
        variant="outline"
        onClick={() => setIndex(index + 1)}
      >
        <ChevronLeftIcon />
      </ActionIcon>
      <Group
        style={{ flexDirection: "row-reverse", flexGrow: 1 }}
        wrap="nowrap"
        justify="space-around"
        gap={0}
      >
        {dates.map((date: Date, i: number) => (
          <MonthToolcard
            key={i}
            date={date}
            cashFlowValue={getCashFlowValue(props.timeToMonthlyTotalsMap, date)}
            isSelected={
              !!props.selectedDates.find((item) => {
                return item.getTime() === date.getTime();
              })
            }
            isPending={props.isPending}
            handleClick={handleClick}
          />
        ))}
      </Group>
      <ActionIcon
        h="62px"
        w="32px"
        flex="0 0"
        variant="outline"
        disabled={index <= 0 && !props.allowFutureMonths}
        onClick={() => setIndex(index - 1)}
      >
        <ChevronRightIcon />
      </ActionIcon>
    </Group>
  );
};

export default MonthToolcards;
