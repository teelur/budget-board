import classes from "./UnbudgetedChildItem.module.css";

import { Group } from "@mantine/core";
import { ActionIcon, AmountText, BodyText } from "@teelur/budget-board-ui";
import { PlusIcon } from "lucide-react";
import { SignDisplay } from "~/helpers/currency";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface UnbudgetedChildItemProps {
  selectedDate: Date | null;
  category: string;
  amount: number;
  isIncome: boolean;
  icon: string;
}

const UnbudgetedChildItem = (
  props: UnbudgetedChildItemProps,
): React.ReactNode => {
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const createBudgetMutation = useCreateBudgetMutation();

  return (
    <Group
      pl="1rem"
      justify="space-between"
      w="100%"
      style={{ containerType: "inline-size" }}
    >
      <BodyText tone="primary" fw={500} className={classes.text}>
        {props.icon.length > 0 ? `${props.icon} ` : ""}
        {props.category}
      </BodyText>
      <Group gap="0.5rem">
        <AmountText
          amount={props.amount * (props.isIncome ? 1 : -1)}
          disableStatusColor
          size="md"
          isSensitive={isPrivacyModeEnabled}
          currency={preferredCurrency}
          decimalPlaces={0}
          locale={intlLocale}
          signDisplay={SignDisplay.Auto}
          className={classes.text}
        />
        {props.selectedDate && (
          <ActionIcon
            variant="filled"
            color="primary"
            size="compact-xs"
            onClick={(event) => {
              event.stopPropagation();
              createBudgetMutation.mutate([
                {
                  month: dayjs(props.selectedDate!).format("YYYY-MM-DD"),
                  category: props.category,
                  limit: Math.round(Math.abs(props.amount)),
                },
              ]);
            }}
          >
            <PlusIcon size={20} />
          </ActionIcon>
        )}
      </Group>
    </Group>
  );
};

export default UnbudgetedChildItem;
