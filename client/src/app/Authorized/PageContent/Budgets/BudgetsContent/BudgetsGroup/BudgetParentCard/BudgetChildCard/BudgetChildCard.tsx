import classes from "../BudgetParentCard.module.css";

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
import { IBudgetUpdateRequest } from "~/models/budget";
import React from "react";
import { useField } from "@mantine/form";
import { CornerDownRight, TrashIcon } from "lucide-react";
import { getBudgetValueColor } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";
import { useDisclosure } from "@mantine/hooks";

interface BudgetChildCardProps {
  id: string;
  categoryValue: string;
  amount: number;
  limit: number;
  isIncome: boolean;
  doEditBudget: (variables: IBudgetUpdateRequest) => void;
  doDeleteBudget: (id: string) => void;
  isPending: boolean;
}

const BudgetChildCard = (props: BudgetChildCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const newLimitField = useField<number | string>({
    initialValue: props.limit ?? 0,
    validate: (value) => (value !== "" ? null : "Invalid limit"),
  });

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (props.id.length === 0) {
      return;
    }
    props.doEditBudget({
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
        onClick={toggle}
      >
        <LoadingOverlay visible={props.isPending} />
        <Group gap="1rem" align="flex-start" wrap="nowrap">
          <Stack gap={0} w="100%">
            <Group
              justify="space-between"
              align="center"
              style={{ containerType: "inline-size" }}
            >
              <Text className={classes.title} fw={600}>
                {props.categoryValue}
              </Text>
              <Group gap={5} justify="flex-end" align="center">
                <Text className={classes.text} fw={700}>
                  {convertNumberToCurrency(
                    props.amount * (props.isIncome ? 1 : -1),
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
                    {convertNumberToCurrency(props.limit, false)}
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
              <Text
                size="md"
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
                  props.doDeleteBudget(props.id);
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
