import classes from "./UnbudgetedGroup.module.css";

import { Accordion, Group, Stack, Text } from "@mantine/core";
import React from "react";
import UnbudgetedCard from "./UnbudgetedCard/UnbudgetedCard";
import { CategoryNode, ICategory, ICategoryNode } from "~/models/category";
import { convertNumberToCurrency } from "~/helpers/currency";

interface UnbudgetedGroupProps {
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categories: ICategory[];
  selectedDate?: Date;
}

const UnbudgetedGroup = (props: UnbudgetedGroupProps): React.ReactNode => {
  const total =
    props.categoryTree.reduce((acc, category) => {
      const categoryTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase()
      );
      return acc + (categoryTotal ? categoryTotal : 0);
    }, 0) + (props.categoryToTransactionsTotalMap.get("") ?? 0);

  return (
    <Accordion variant="separated" radius="md">
      <Accordion.Item
        className={classes.accordion}
        key="unbudgeted"
        value="unbudgeted"
      >
        <Accordion.Control>
          <Group justify="space-between" align="center" w="100%" pr="1rem">
            <Text size="1.2rem" fw={600}>
              Unbudgeted
            </Text>
            <Text size="1.2rem" fw={600}>
              {convertNumberToCurrency(total)}
            </Text>
          </Group>
        </Accordion.Control>
        <Accordion.Panel p={0}>
          <Stack gap="0.6rem">
            <UnbudgetedCard
              categoryTree={
                new CategoryNode({
                  value: "",
                  parent: "",
                })
              }
              categoryToTransactionsTotalMap={
                props.categoryToTransactionsTotalMap
              }
            />
            {props.categoryTree.length > 0 ? (
              props.categoryTree.map((categoryTree) => (
                <UnbudgetedCard
                  key={categoryTree.value}
                  categoryTree={categoryTree}
                  categoryToTransactionsTotalMap={
                    props.categoryToTransactionsTotalMap
                  }
                  selectedDate={props.selectedDate}
                />
              ))
            ) : (
              <Text>No unbudgeted transactions found.</Text>
            )}
          </Stack>
        </Accordion.Panel>
      </Accordion.Item>
    </Accordion>
  );
};

export default UnbudgetedGroup;
