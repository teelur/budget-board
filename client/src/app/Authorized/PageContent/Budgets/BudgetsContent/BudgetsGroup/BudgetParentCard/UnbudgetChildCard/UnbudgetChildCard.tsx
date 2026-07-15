import classes from "./UnbudgetChildCard.module.css";

import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { ActionIcon, Group, LoadingOverlay } from "@mantine/core";
import { CornerDownRight, PlusIcon } from "lucide-react";
import React from "react";
import { roundAwayFromZero } from "~/helpers/utils";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface UnbudgetChildCardProps {
  selectedDate: Date | null;
  category: string;
  amount: number;
  isIncome: boolean;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetChildCard = (props: UnbudgetChildCardProps): React.ReactNode => {
  const { intlLocale, dayjs } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const createBudgetMutation = useCreateBudgetMutation();

  if (roundAwayFromZero(props.amount) === 0) {
    return null;
  }

  return (
    <Group wrap="nowrap">
      <CornerDownRight />
      <Card
        w="100%"
        p="0.25rem"
        onClick={() => {
          if (props.selectedDate) {
            props.openDetails(props.category, props.selectedDate);
          }
        }}
        hoverEffect
        elevation={1}
      >
        <LoadingOverlay visible={createBudgetMutation.isPending} />
        <Group
          justify="space-between"
          w="100%"
          style={{ containerType: "inline-size" }}
        >
          <PrimaryText className={classes.text} elevation={1}>
            {props.category}
          </PrimaryText>
          <Group gap="0.5rem">
            <PrimaryText className={classes.text} elevation={1}>
              {convertNumberToCurrency(
                props.amount * (props.isIncome ? 1 : -1),
                false,
                preferredCurrency,
                SignDisplay.Auto,
                intlLocale,
              )}
            </PrimaryText>
            {props.selectedDate && (
              <ActionIcon
                size="sm"
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
                <PlusIcon />
              </ActionIcon>
            )}
          </Group>
        </Group>
      </Card>
    </Group>
  );
};

export default UnbudgetChildCard;
