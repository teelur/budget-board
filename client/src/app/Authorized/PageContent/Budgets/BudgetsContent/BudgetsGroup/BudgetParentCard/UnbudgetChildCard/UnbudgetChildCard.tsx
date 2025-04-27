import classes from "./UnbudgetChildCard.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import { ActionIcon, Card, Group, LoadingOverlay, Text } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { CornerDownRight, PlusIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { roundAwayFromZero } from "~/helpers/utils";

interface UnbudgetChildCardProps {
  selectedDate?: Date;
  category: string;
  amount: number;
  isIncome: boolean;
}

const UnbudgetChildCard = (props: UnbudgetChildCardProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doAddBudget = useMutation({
    mutationFn: async (newBudget: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudget,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["budgets"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  if (roundAwayFromZero(props.amount) === 0) {
    return null;
  }

  return (
    <Group wrap="nowrap">
      <CornerDownRight />
      <Card p="0.5rem" radius="md" w="100%">
        <LoadingOverlay visible={doAddBudget.isPending} />
        <Group
          justify="space-between"
          w="100%"
          style={{ containerType: "inline-size" }}
        >
          <Text className={classes.text} fw={600}>
            {props.category}
          </Text>
          <Group gap="sm">
            <Text className={classes.text} fw={600}>
              {convertNumberToCurrency(
                props.amount * (props.isIncome ? 1 : -1),
                false
              )}
            </Text>
            {props.selectedDate && props.category !== "Uncategorized" && (
              <ActionIcon
                size="sm"
                onClick={() =>
                  doAddBudget.mutate([
                    {
                      date: props.selectedDate!,
                      category: props.category,
                      limit: Math.round(Math.abs(props.amount)),
                    },
                  ])
                }
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
