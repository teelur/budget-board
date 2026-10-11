import { Stack } from "@mantine/core";
import { BodyText, Card } from "@teelur/budget-board-ui";
import { CashFlowValue } from "~/models/budget";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import classes from "./MonthToolcard.module.css";

interface MonthToolcardProps {
  date: Date;
  cashFlowValue: CashFlowValue;
  isSelected: boolean;
  isPending?: boolean;
  handleClick: (date: Date) => void;
}

const MonthToolcard = (props: MonthToolcardProps): React.ReactNode => {
  const { dayjs } = useLocale();

  const cashFlowState = props.isPending
    ? "pending"
    : CashFlowValue[props.cashFlowValue].toLowerCase();

  const getLightColor = (
    cashFlowValue: CashFlowValue,
    isSelected: boolean,
    isPending?: boolean,
  ): string => {
    if (isSelected) {
      if (isPending) {
        return "var(--light-color-off)";
      }

      switch (cashFlowValue) {
        case CashFlowValue.Positive:
          return "var(--button-color-confirm)";
        case CashFlowValue.Neutral:
          return "var(--light-color-off)";
        case CashFlowValue.Negative:
          return "var(--button-color-destructive)";
      }
    }
    return "var(--light-color-off)";
  };

  return (
    <Card
      h="62px"
      w="60px"
      flex="0 0 auto"
      p="0.125rem"
      onClick={() => props.handleClick(props.date)}
      hoverable={false}
      className={classes.card}
      selected={props.isSelected}
    >
      <Stack gap={0} style={{ userSelect: "none" }}>
        <Card
          h="20px"
          w="100%"
          bg={getLightColor(
            props.cashFlowValue,
            props.isSelected,
            props.isPending,
          )}
          withBorder={false}
          shadow="none"
          className={classes.statusStrip}
          data-cash-flow={cashFlowState}
        />
        <BodyText tone="primary" size="sm" fw={500}>
          {dayjs(props.date).format("MMM")}
        </BodyText>
        <BodyText tone="secondary" size="xs">
          {dayjs(props.date).format("YYYY")}
        </BodyText>
      </Stack>
    </Card>
  );
};

export default MonthToolcard;
