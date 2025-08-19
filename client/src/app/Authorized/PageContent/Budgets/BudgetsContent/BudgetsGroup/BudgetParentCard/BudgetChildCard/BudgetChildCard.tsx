import classes from "./BudgetChildCard.module.css";

import { convertNumberToCurrency, getCurrencySymbol } from "~/helpers/currency";
import {
  ActionIcon,
  Card,
  Flex,
  Group,
  LoadingOverlay,
  NumberInput,
  Progress,
  Stack,
  Text,
} from "@mantine/core";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import React from "react";
import { useField } from "@mantine/form";
import { CornerDownRight, PencilIcon, TrashIcon } from "lucide-react";
import { getBudgetValueColor } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IUserSettings } from "~/models/userSettings";

interface BudgetChildCardProps {
  id: string;
  categoryValue: string;
  amount: number;
  limit: number;
  isIncome: boolean;
  selectedDate: Date;
  openDetails: (category: string, month: Date) => void;
}

const BudgetChildCard = (props: BudgetChildCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const newLimitField = useField<number | string>({
    initialValue: props.limit ?? 0,
    validate: (value) => (value !== "" ? null : "Invalid limit"),
  });

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
  const doEditBudget = useMutation({
    mutationFn: async (newBudget: IBudgetUpdateRequest) =>
      await request({
        url: "/api/budget",
        method: "PUT",
        data: newBudget,
      }),
    onMutate: async (variables: IBudgetUpdateRequest) => {
      await queryClient.cancelQueries({ queryKey: ["budgets"] });

      const previousBudgets: IBudget[] =
        queryClient.getQueryData(["budgets"]) ?? [];

      queryClient.setQueryData(["budgets"], (oldBudgets: IBudget[]) =>
        oldBudgets?.map((oldBudget) =>
          oldBudget.id === variables.id
            ? { ...oldBudget, limit: variables.limit }
            : oldBudget
        )
      );

      return { previousBudgets };
    },
    onError: (error: AxiosError, _variables: IBudgetUpdateRequest, context) => {
      queryClient.setQueryData(["budgets"], context?.previousBudgets ?? []);
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: ["budgets"] }),
  });

  const doDeleteBudget = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/budget",
        method: "DELETE",
        params: { guid: id },
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["budgets"] }),
  });

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (props.id.length === 0) {
      return;
    }
    doEditBudget.mutate({
      id: props.id,
      limit: Number(newLimit),
    });
  };

  const percentComplete = roundAwayFromZero(
    ((props.amount * (props.isIncome ? 1 : -1)) / props.limit) * 100
  );

  return (
    <Group wrap="nowrap">
      <CornerDownRight />
      <Card
        className={classes.budgetCard}
        p="0.25rem 0.5rem"
        w="100%"
        radius="md"
        bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
        shadow="md"
        onClick={() => {
          if (props.id.length > 0) {
            props.openDetails(props.categoryValue, props.selectedDate);
          }
        }}
      >
        <LoadingOverlay
          visible={doEditBudget.isPending || doDeleteBudget.isPending}
        />
        <Group gap="1rem" align="flex-start" wrap="nowrap">
          <Stack gap={0} w="100%">
            <Group
              justify="space-between"
              align="center"
              style={{ containerType: "inline-size" }}
            >
              <Group gap={5} align="center">
                <Text className={classes.title} fw={600}>
                  {props.categoryValue}
                </Text>
                <ActionIcon
                  variant={isSelected ? "outline" : "transparent"}
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (props.id.length > 0) {
                      newLimitField.setValue(props.limit);
                      toggle();
                    }
                  }}
                >
                  <PencilIcon size={16} />
                </ActionIcon>
              </Group>
              <Group gap={5} justify="flex-end" align="center">
                {userSettingsQuery.isPending ? null : (
                  <Text className={classes.text} fw={700}>
                    {convertNumberToCurrency(
                      props.amount * (props.isIncome ? 1 : -1),
                      false,
                      userSettingsQuery.data?.currency ?? "USD"
                    )}
                  </Text>
                )}
                <Text className={classes.textSmall} fw={600}>
                  {" "}
                  of{" "}
                </Text>
                {isSelected ? (
                  <Flex onClick={(e) => e.stopPropagation()}>
                    <NumberInput
                      {...newLimitField.getInputProps()}
                      onBlur={() => handleEdit(newLimitField.getValue())}
                      min={0}
                      max={999999}
                      step={1}
                      prefix={getCurrencySymbol(
                        userSettingsQuery.data?.currency
                      )}
                      placeholder="Limit"
                      radius="md"
                      size="xs"
                      styles={{
                        root: {
                          maxWidth: "100px",
                        },
                        input: {
                          padding: "0 10px",
                          fontSize: "16px",
                        },
                      }}
                    />
                  </Flex>
                ) : userSettingsQuery.isPending ? null : (
                  <Text className={classes.text} fw={700}>
                    {convertNumberToCurrency(
                      props.limit,
                      false,
                      userSettingsQuery.data?.currency ?? "USD"
                    )}
                  </Text>
                )}
              </Group>
            </Group>
            <Group
              gap={5}
              justify="flex-end"
              align="center"
              style={{ containerType: "inline-size" }}
            >
              <Flex style={{ flex: "1 1 auto" }}>
                <Progress.Root size={12} radius="xl" w="100%">
                  <Progress.Section
                    value={percentComplete}
                    color={getBudgetValueColor(
                      roundAwayFromZero(props.amount),
                      props.limit,
                      props.isIncome
                    )}
                  >
                    <Progress.Label>
                      {percentComplete.toFixed(0)}%
                    </Progress.Label>
                  </Progress.Section>
                </Progress.Root>
              </Flex>
              {userSettingsQuery.isPending ? null : (
                <Text
                  size="0.9rem"
                  fw={700}
                  c={getBudgetValueColor(
                    roundAwayFromZero(props.amount),
                    props.limit,
                    props.isIncome
                  )}
                >
                  {convertNumberToCurrency(
                    roundAwayFromZero(
                      props.limit - props.amount * (props.isIncome ? 1 : -1)
                    ),
                    false,
                    userSettingsQuery.data?.currency ?? "USD"
                  )}
                </Text>
              )}
              <Text size="sm" fw={600}>
                {" "}
                left
              </Text>
            </Group>
          </Stack>
          {isSelected && (
            <Group style={{ alignSelf: "stretch" }}>
              <ActionIcon
                color="red"
                onClick={(e) => {
                  e.stopPropagation();
                  doDeleteBudget.mutate(props.id);
                }}
                h="100%"
              >
                <TrashIcon size="1rem" />
              </ActionIcon>
            </Group>
          )}
        </Group>
      </Card>
    </Group>
  );
};

export default BudgetChildCard;
