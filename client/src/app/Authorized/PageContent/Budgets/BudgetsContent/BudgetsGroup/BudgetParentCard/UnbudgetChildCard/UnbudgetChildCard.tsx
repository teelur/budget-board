import classes from "./UnbudgetChildCard.module.css";
import hoverClasses from "~/styles/Hoverable.module.css";

import { ActionIcon, Box, Group, LoadingOverlay } from "@mantine/core";
import { PlusIcon } from "lucide-react";
import React from "react";
import { roundAwayFromZero } from "~/helpers/utils";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";

interface UnbudgetChildCardProps {
  selectedDate: Date | null;
  category: string;
  amount: number;
  isIncome: boolean;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetChildCard = (props: UnbudgetChildCardProps): React.ReactNode => {
  const { dayjs } = useLocale();
  const createBudgetMutation = useCreateBudgetMutation();

  if (roundAwayFromZero(props.amount) === 0) {
    return null;
  }

  return (
    <Box
      mx="0.25rem"
      my="0.125rem"
      p="0.25rem"
      pl="1.5rem"
      data-hover-effect="true"
      className={`${classes.row} ${hoverClasses.hoverable} ${hoverClasses.outline}`}
      onClick={() => {
        if (props.selectedDate) {
          props.openDetails(props.category, props.selectedDate);
        }
      }}
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
            <SensitiveAmount
              amount={props.amount * (props.isIncome ? 1 : -1)}
              includeCents={false}
            />
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
    </Box>
  );
};

export default UnbudgetChildCard;
