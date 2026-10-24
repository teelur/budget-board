import classes from "./UnbudgetedChildCard.module.css";

import { Group } from "@mantine/core";
import {
  ActionIcon,
  AmountText,
  BodyText,
  Card,
  LoadingOverlay,
} from "@teelur/budget-board-ui";
import { CornerDownRightIcon, PlusIcon } from "lucide-react";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import React from "react";
import { roundAwayFromZero } from "~/helpers/utils";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { SignDisplay } from "~/helpers/currency";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";

interface UnbudgetedChildCardProps {
  category: string;
  amount: number;
  selectedDate: Date | null;
  icon: string;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedChildCard = (
  props: UnbudgetedChildCardProps,
): React.ReactNode => {
  const { dayjs, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const createBudgetMutation = useCreateBudgetMutation();

  if (roundAwayFromZero(props.amount) === 0) {
    return null;
  }

  return (
    <Group w="100%" align="center" wrap="nowrap" gap="0.5rem">
      <CornerDownRightIcon size={20} />
      <Card
        surface="elevated"
        p="0.25rem"
        w="100%"
        onClick={() => {
          if (props.selectedDate) {
            props.openDetails(props.category, props.selectedDate);
          }
        }}
        hoverable
      >
        <LoadingOverlay visible={createBudgetMutation.isPending} />
        <Group w="100%" justify="space-between">
          <BodyText fw={500} className={classes.text}>
            {props.icon.length > 0 ? `${props.icon} ` : ""}
            {props.category}
          </BodyText>
          <Group gap="sm">
            <AmountText
              amount={props.amount}
              size="sm"
              isSensitive={isPrivacyModeEnabled}
              currency={preferredCurrency}
              decimalPlaces={0}
              locale={intlLocale}
              signDisplay={SignDisplay.Auto}
            />
            {props.selectedDate && (
              <ActionIcon
                variant="filled"
                color="primary"
                size="compact-xs"
                onClick={(e) => {
                  e.stopPropagation();
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
      </Card>
    </Group>
  );
};

export default UnbudgetedChildCard;
