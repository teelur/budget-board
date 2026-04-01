import classes from "./UnbudgetedChildCard.module.css";

import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { ActionIcon, Group, LoadingOverlay } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { CornerDownRightIcon, PlusIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { roundAwayFromZero } from "~/helpers/utils";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface UnbudgetedChildCardProps {
  category: string;
  amount: number;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedChildCard = (
  props: UnbudgetedChildCardProps,
): React.ReactNode => {
  const { intlLocale } = useLocale();
  const { request } = useAuth();

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
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

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
        <LoadingOverlay visible={doAddBudget.isPending} />
        <Group w="100%" justify="space-between">
          <PrimaryText className={classes.text}>{props.category}</PrimaryText>
          <Group gap="sm">
            {userSettingsQuery.isPending ? null : (
              <PrimaryText className={classes.text}>
                {convertNumberToCurrency(
                  props.amount,
                  false,
                  userSettingsQuery.data?.currency ?? "USD",
                  SignDisplay.Auto,
                  intlLocale,
                )}
              </PrimaryText>
            )}
            {props.selectedDate && (
              <ActionIcon
                size="sm"
                onClick={(e) => {
                  e.stopPropagation();
                  doAddBudget.mutate([
                    {
                      date: props.selectedDate!,
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
