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

export interface BudgetParentCardProps {
  categoryTree: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
  categoryToTransactionsTotalMap: Map<string, number>;
  doEditBudget: (variables: IBudgetUpdateRequest) => void;
  doDeleteBudget: (id: string) => void;
  isPending: boolean;
}

const BudgetParentCard = (props: BudgetParentCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const isIncome = areStringsEqual(props.categoryTree.value, "income");
  const limit = props.categoryToLimitsMap.get(props.categoryTree.value) ?? 0;
  const amount =
    props.categoryToTransactionsTotalMap.get(props.categoryTree.value) ?? 0;

  const newLimitField = useField<number | string>({
    initialValue: limit ?? 0,
    validate: (value) => (value !== "" ? null : "Invalid limit"),
  });

  const percentComplete = roundAwayFromZero(
    (((props.categoryToTransactionsTotalMap.get(props.categoryTree.value) ??
      0) *
      (isIncome ? 1 : -1)) /
      limit) *
      100
  );

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if ("".length === 0) {
      return;
    }
    props.doEditBudget({
      id: "",
      limit: Number(newLimit),
    });
  };

  return (
    <Card
      className={classes.root}
      p="0.5rem"
      w="100%"
      radius="md"
      onClick={toggle}
      bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
      shadow="md"
    >
      <LoadingOverlay visible={props.isPending} />
      <Stack gap={0}>
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
                  {convertNumberToCurrency(0 * (isIncome ? 1 : -1), false)}
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
              align="center"
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
        <Stack gap={5}>
          {props.categoryTree.subCategories.map((subCategory) => (
            <BudgetChildCard
              key={subCategory.value}
              id=""
              categoryValue={subCategory.value}
              amount={0}
              limit={0}
              isIncome={false}
              doEditBudget={props.doEditBudget}
              isSelected={isSelected}
              doDeleteBudget={props.doDeleteBudget}
              isPending={props.isPending}
            />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default BudgetParentCard;
