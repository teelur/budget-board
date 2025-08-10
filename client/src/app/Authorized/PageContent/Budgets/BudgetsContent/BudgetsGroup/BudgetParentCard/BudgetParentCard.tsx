import classes from "./BudgetParentCard.module.css";

import { convertNumberToCurrency, getCurrencySymbol } from "~/helpers/currency";
import {
  ActionIcon,
  Button,
  Card,
  Flex,
  Group,
  LoadingOverlay,
  NumberInput,
  Popover,
  Progress,
  Stack,
  Text,
} from "@mantine/core";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import { useField } from "@mantine/form";
import { PencilIcon, TrashIcon } from "lucide-react";
import { getBudgetValueColor } from "~/helpers/budgets";
import { areStringsEqual, roundAwayFromZero } from "~/helpers/utils";
import { ICategoryNode } from "~/models/category";
import BudgetChildCard from "./BudgetChildCard/BudgetChildCard";
import UnbudgetChildCard from "./UnbudgetChildCard/UnbudgetChildCard";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IUserSettings } from "~/models/userSettings";

export interface BudgetParentCardProps {
  categoryTree: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const BudgetParentCard = (props: BudgetParentCardProps): React.ReactNode => {
  const [isSelected, { toggle, close }] = useDisclosure(false);

  const isIncome = areStringsEqual(props.categoryTree.value, "income");
  const limit =
    props.categoryToLimitsMap.get(props.categoryTree.value.toLowerCase()) ?? 0;
  const amount =
    props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase()
    ) ?? 0;

  const budgets =
    props.categoryToBudgetsMap.get(props.categoryTree.value.toLowerCase()) ??
    [];
  const id =
    budgets.length === 1 && props.selectedDate ? budgets[0]?.id ?? "" : "";

  const newLimitField = useField<number | string>({
    initialValue: limit ?? 0,
    validate: (value) => (value !== "" ? null : "Invalid limit"),
  });

  const percentComplete = roundAwayFromZero(
    (((props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase()
    ) ?? 0) *
      (isIncome ? 1 : -1)) /
      limit) *
      100
  );

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
    if (id.length === 0) {
      return;
    }
    doEditBudget.mutate({
      id,
      limit: Number(newLimit),
    });
  };

  const childLimitsTotal = props.categoryTree.subCategories.reduce(
    (acc, subCategory) => {
      const limit =
        props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ?? 0;
      return acc + limit;
    },
    0
  );

  interface ChildCards {
    budgetChildCards: React.ReactNode[];
    unbudgetChildCards: React.ReactNode[];
  }

  const buildChildren = (): ChildCards => {
    const budgetChildCards: React.ReactNode[] = [];
    const unbudgetChildCards: React.ReactNode[] = [];

    props.categoryTree.subCategories.forEach((subCategory) => {
      if (
        props.categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase())
      ) {
        const budgets =
          props.categoryToBudgetsMap.get(
            subCategory.value.toLocaleLowerCase()
          ) ?? [];
        const budgetId =
          budgets.length === 1 && props.selectedDate
            ? budgets[0]?.id ?? ""
            : "";
        budgetChildCards.push(
          <BudgetChildCard
            key={subCategory.value}
            id={budgetId}
            categoryValue={subCategory.value}
            amount={
              props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase()
              ) ?? 0
            }
            limit={
              props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ??
              0
            }
            isIncome={isIncome}
            selectedDate={props.selectedDate ?? new Date()}
            openDetails={props.openDetails}
          />
        );
      } else if (
        props.categoryToTransactionsTotalMap.has(
          subCategory.value.toLocaleLowerCase()
        )
      ) {
        unbudgetChildCards.push(
          <UnbudgetChildCard
            key={subCategory.value}
            category={subCategory.value}
            amount={
              props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase()
              ) ?? 0
            }
            selectedDate={props.selectedDate}
            isIncome={isIncome}
            openDetails={props.openDetails}
          />
        );
      }
    });

    return { budgetChildCards, unbudgetChildCards };
  };

  const { budgetChildCards, unbudgetChildCards } = buildChildren();

  return (
    <Card
      className={classes.root}
      bg="var(--mantine-color-content-background)"
      p="0.25rem"
      w="100%"
      radius="md"
    >
      <Stack gap={5}>
        <Card
          className={classes.budgetCard}
          p="0.25rem 0.5rem"
          radius="md"
          bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
          shadow="md"
          onClick={() => {
            if (id.length > 0) {
              props.openDetails(props.categoryTree.value, props.selectedDate);
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
                    {props.categoryTree.value}
                  </Text>
                  <ActionIcon
                    variant="transparent"
                    size="md"
                    onClick={(e) => {
                      e.stopPropagation();
                      if (id.length > 0) {
                        newLimitField.setValue(limit);
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
                        amount * (isIncome ? 1 : -1),
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
                        min={childLimitsTotal}
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
                        limit,
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
                align="baseline"
                style={{ containerType: "inline-size" }}
              >
                <Flex style={{ flex: "1 1 auto" }}>
                  <Progress.Root size={16} radius="xl" w="100%">
                    <Progress.Section
                      value={percentComplete}
                      color={getBudgetValueColor(
                        roundAwayFromZero(amount),
                        limit,
                        isIncome
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
                    size="md"
                    fw={700}
                    c={getBudgetValueColor(
                      roundAwayFromZero(amount),
                      limit,
                      isIncome
                    )}
                  >
                    {convertNumberToCurrency(
                      roundAwayFromZero(limit - amount * (isIncome ? 1 : -1)),
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
              <Flex
                style={{ alignSelf: "stretch" }}
                onClick={(e) => e.stopPropagation()}
              >
                <Popover>
                  <Popover.Target>
                    <ActionIcon color="red" h="100%">
                      <TrashIcon size="1rem" />
                    </ActionIcon>
                  </Popover.Target>
                  <Popover.Dropdown p="0.5rem" maw={200}>
                    <Stack gap={5}>
                      <Text size="sm" fw={500}>
                        Are you sure you want to delete this budget?
                      </Text>
                      <Text size="sm" fw={500}>
                        All children will also be deleted.
                      </Text>
                      <Button
                        color="red"
                        size="compact-xs"
                        onClick={() => {
                          doDeleteBudget.mutate(id);
                          close();
                        }}
                      >
                        Delete
                      </Button>
                    </Stack>
                  </Popover.Dropdown>
                </Popover>
              </Flex>
            )}
          </Group>
        </Card>
        {props.categoryTree.subCategories.length > 0 &&
          (budgetChildCards.length > 0 || unbudgetChildCards.length > 0) && (
            <Stack gap={5}>
              {budgetChildCards.length > 0 && budgetChildCards}
              {unbudgetChildCards.length > 0 && unbudgetChildCards}
            </Stack>
          )}
      </Stack>
    </Card>
  );
};

export default BudgetParentCard;
