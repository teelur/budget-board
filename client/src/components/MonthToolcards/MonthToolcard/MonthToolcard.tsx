import { Stack } from "@mantine/core";
import { CashFlowValue } from "~/models/budget";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface MonthToolcardProps {
  date: Date;
  cashFlowValue: CashFlowValue;
  isSelected: boolean;
  isPending?: boolean;
  handleClick: (date: Date) => void;
}

const MonthToolcard = (props: MonthToolcardProps): React.ReactNode => {
  const { dayjs } = useLocale();

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
      hoverEffect
      elevation={1}
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
          elevation={1}
        />
        <PrimaryText size="sm">{dayjs(props.date).format("MMM")}</PrimaryText>
        <DimmedText size="xs" c="dimmed">
          {dayjs(props.date).format("YYYY")}
        </DimmedText>
      </Stack>
    </Card>
  );
};

export default MonthToolcard;
