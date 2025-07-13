import classes from "./UnbudgetedChildCard.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import { ActionIcon, Card, Group, LoadingOverlay, Text } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { CornerDownRightIcon, PlusIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { roundAwayFromZero } from "~/helpers/utils";
import { IUserSettings } from "~/models/userSettings";

interface UnbudgetedChildCardProps {
  category: string;
  amount: number;
  selectedDate: Date | null;
}

const UnbudgetedChildCard = (
  props: UnbudgetedChildCardProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

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
    <Group w="100%" align="center" wrap="nowrap" gap="0.5rem">
      <CornerDownRightIcon size={20} />
      <Card className={classes.root} radius="md" p="0.5rem" w="100%">
        <LoadingOverlay visible={doAddBudget.isPending} />
        <Group w="100%" justify="space-between">
          <Text className={classes.text} fw={600}>
            {props.category}
          </Text>
          <Group gap="sm">
            {userSettingsQuery.isPending ? null : (
              <Text className={classes.text} fw={600}>
                {convertNumberToCurrency(
                  props.amount,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </Text>
            )}
            {props.selectedDate && (
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

export default UnbudgetedChildCard;
