import classes from "./UnbudgetedChildCard.module.css";

import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { ActionIcon, Group, LoadingOverlay } from "@mantine/core";
import { CornerDownRightIcon, PlusIcon } from "lucide-react";
import React from "react";
import { roundAwayFromZero } from "~/helpers/utils";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface UnbudgetedChildCardProps {
  category: string;
  amount: number;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedChildCard = (
  props: UnbudgetedChildCardProps,
): React.ReactNode => {
  const { intlLocale, dayjs } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const createBudgetMutation = useCreateBudgetMutation();

  if (roundAwayFromZero(props.amount) === 0) {
    return null;
  }

  return (
    <Group w="100%" align="center" wrap="nowrap" gap="0.5rem">
      <CornerDownRightIcon size={20} />
      <Card
        p="0.25rem"
        w="100%"
        onClick={() => {
          if (props.selectedDate) {
            props.openDetails(props.category, props.selectedDate);
          }
        }}
        hoverEffect
        elevation={2}
      >
        <LoadingOverlay visible={createBudgetMutation.isPending} />
        <Group w="100%" justify="space-between">
          <PrimaryText className={classes.text}>{props.category}</PrimaryText>
          <Group gap="sm">
            <PrimaryText className={classes.text}>
              {convertNumberToCurrency(
                props.amount,
                false,
                preferredCurrency,
                SignDisplay.Auto,
                intlLocale,
              )}
            </PrimaryText>
            {props.selectedDate && (
              <ActionIcon
                size="sm"
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
                <PlusIcon />
              </ActionIcon>
            )}
          </Group>
        </Group>
      </Card>
    </Group>
  );
};

export default UnbudgetedChildCard;
