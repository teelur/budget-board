import classes from "./BudgetCard.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import {
  Button,
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
import { useDisclosure } from "@mantine/hooks";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { AxiosError } from "axios";
import { notifications } from "@mantine/notifications";
import { useField } from "@mantine/form";
import { Trash2Icon } from "lucide-react";
import { getBudgetValueColor } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";

interface BudgetCardProps {
  budgets: IBudget[];
  categoryDisplayString: string;
  amount: number;
  isIncome: boolean;
}

const BudgetCard2 = (props: BudgetCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const newLimitField = useField<number | string>({
    initialValue: props.budgets[0]?.limit ?? 0,
    validate: (value) => (value !== "" ? null : "Invalid limit"),
  });

  const { request } = React.useContext<any>(AuthContext);
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

  const limit = React.useMemo(
    () => props.budgets.reduce((n: number, b: IBudget) => n + b.limit, 0),
    [props.budgets]
  );

  const percentComplete = roundAwayFromZero(
    ((props.amount * (props.isIncome ? 1 : -1)) / limit) * 100
  );

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (props.budgets.length === 0) {
      return;
    }
    doEditBudget.mutate({
      id: props.budgets[0]!.id,
      limit: Number(newLimit),
    });
  };

  return (
    <Card
      className={classes.root}
      radius="md"
      onClick={toggle}
      bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
      shadow="md"
    >
      <LoadingOverlay
        visible={doEditBudget.isPending || doDeleteBudget.isPending}
      />
      <Stack className={classes.budgetContainer}>
        <Group className={classes.dataContainer}>
          <Group className={classes.budgetNameContainer}>
            <Text className={classes.text}>{props.categoryDisplayString}</Text>
            {isSelected && (
              <Button
                size="compact-sm"
                bg="red"
                onClick={(e) => {
                  e.stopPropagation();
                  doDeleteBudget.mutate(props.budgets[0]!.id);
                }}
              >
                <Group gap="0.125rem">
                  <Trash2Icon size={18} />
                  <Text>Delete</Text>
                </Group>
              </Button>
            )}
          </Group>
          <Group className={classes.budgetValuesContainer}>
            <Flex className={classes.numberContainer}>
              <Text className={classes.text}>
                {convertNumberToCurrency(
                  props.amount * (props.isIncome ? 1 : -1),
                  false
                )}
              </Text>
            </Flex>
            <Flex className={classes.numberContainer}>
              {isSelected ? (
                <Flex onClick={(e) => e.stopPropagation()}>
                  <NumberInput
                    {...newLimitField.getInputProps()}
                    onBlur={() => handleEdit(newLimitField.getValue())}
                    min={0}
                    max={999999}
                    step={1}
                    prefix="$"
                    placeholder="Limit"
                    radius="md"
                    styles={{
                      input: {
                        padding: "0 10px",
                        fontSize: "14px",
                        lineHeight: "20px",
                      },
                    }}
                  />
                </Flex>
              ) : (
                <Text className={classes.text}>
                  {convertNumberToCurrency(limit, false)}
                </Text>
              )}
            </Flex>
            <Flex className={classes.numberContainer}>
              <Text
                className={classes.text}
                c={getBudgetValueColor(
                  roundAwayFromZero(props.amount),
                  limit,
                  props.isIncome
                )}
              >
                {convertNumberToCurrency(
                  roundAwayFromZero(
                    limit - props.amount * (props.isIncome ? 1 : -1)
                  ),
                  false
                )}
              </Text>
            </Flex>
          </Group>
        </Group>
        <Progress.Root size={16} radius="xl">
          <Progress.Section
            value={percentComplete}
            color={getBudgetValueColor(
              roundAwayFromZero(props.amount),
              limit,
              props.isIncome
            )}
          >
            <Progress.Label>{percentComplete.toFixed(0)}%</Progress.Label>
          </Progress.Section>
        </Progress.Root>
      </Stack>
    </Card>
  );
};

export default BudgetCard2;
