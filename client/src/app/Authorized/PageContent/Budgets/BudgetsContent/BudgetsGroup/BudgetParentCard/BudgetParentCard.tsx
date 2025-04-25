import classes from "./BudgetParentCard.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
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
import { useDisclosure } from "@mantine/hooks";
import { useField } from "@mantine/form";
import { TrashIcon } from "lucide-react";
import { getBudgetValueColor } from "~/helpers/budgets";
import { areStringsEqual, roundAwayFromZero } from "~/helpers/utils";
import { ICategoryNode } from "~/models/category";
import BudgetChildCard from "./BudgetChildCard/BudgetChildCard";
import UnbudgetChildCard from "./UnbudgetChildCard/UnbudgetChildCard";

export interface BudgetParentCardProps {
  categoryTree: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
  categoryToTransactionsTotalMap: Map<string, number>;
  doEditBudget: (variables: IBudgetUpdateRequest) => void;
  doDeleteBudget: (id: string) => void;
  isPending: boolean;
  selectedDate?: Date;
}

const BudgetParentCard = (props: BudgetParentCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

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
  const id = budgets.length === 1 ? budgets[0]?.id ?? "" : "";

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

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (id.length === 0) {
      return;
    }
    props.doEditBudget({
      id,
      limit: Number(newLimit),
    });
  };

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
        const budget = budgets[0] ?? null;
        const budgetId = budget?.id ?? "";
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
            doEditBudget={props.doEditBudget}
            doDeleteBudget={props.doDeleteBudget}
            isPending={props.isPending}
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
          />
        );
      }
    });

    return { budgetChildCards, unbudgetChildCards };
  };

  const { budgetChildCards, unbudgetChildCards } = buildChildren();

  return (
    <Card className={classes.root} p="0.25rem" w="100%" radius="md">
      <LoadingOverlay visible={props.isPending} />
      <Stack gap={5}>
        <Card
          className={classes.budgetCard}
          p="0.25rem 0.5rem"
          radius="md"
          bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
          onClick={() => id.length > 0 && toggle()}
          shadow="md"
        >
          <Group gap="1rem" align="flex-start" wrap="nowrap">
            <Stack gap={0} w="100%">
              <Group
                justify="space-between"
                align="center"
                style={{ containerType: "inline-size" }}
              >
                <Text className={classes.title} fw={600}>
                  {props.categoryTree.value}
                </Text>
                <Group gap={5} justify="flex-end" align="center">
                  <Text className={classes.text} fw={700}>
                    {convertNumberToCurrency(
                      amount * (isIncome ? 1 : -1),
                      false
                    )}
                  </Text>
                  <Text className={classes.textSmall}> of </Text>
                  {/* TODO: Limit min should be the total of all children if it is a parent */}
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
                  ) : (
                    <Text className={classes.text} fw={700}>
                      {convertNumberToCurrency(limit, false)}
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
                    false
                  )}
                </Text>
                <Text size="md"> left</Text>
              </Group>
            </Stack>
            {isSelected && (
              <Group style={{ alignSelf: "stretch" }}>
                <ActionIcon
                  color="red"
                  onClick={(e) => {
                    e.stopPropagation();
                    props.doDeleteBudget("");
                  }}
                  h="100%"
                >
                  <TrashIcon size="1rem" />
                </ActionIcon>
              </Group>
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
