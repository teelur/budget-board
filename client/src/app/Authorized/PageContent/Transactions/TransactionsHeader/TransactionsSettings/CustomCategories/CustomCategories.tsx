import classes from "./CustomCategories.module.css";

import { Flex, Group, Stack, Text } from "@mantine/core";
import React from "react";
import AddCategory from "./AddCategory/AddCategory";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ICategoryResponse } from "~/models/category";
import CustomCategoryCard from "./CustomCategoryCard/CustomCategoryCard";

const CustomCategories = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  return (
    <Stack gap="0.5rem">
      <Text c="dimmed" size="sm" fw={600}>
        Create custom categories to organize your transactions.
      </Text>
      <AddCategory />
      <Stack>
        <Group px="0.5rem" justify="space-between">
          <Flex className={classes.nameContainer}>
            <Text fw={600}>Name</Text>
          </Flex>
          <Flex className={classes.parentContainer}>
            <Text fw={600}>Parent</Text>
          </Flex>
          <Flex className={classes.deleteContainer}>
            <Text />
          </Flex>
        </Group>
        {(transactionCategoriesQuery.data ?? []).length > 0 ? (
          transactionCategoriesQuery.data?.map(
            (category: ICategoryResponse) => (
              <CustomCategoryCard key={category.id} category={category} />
            )
          )
        ) : (
          <Text fw={600} size="sm">
            No custom categories.
          </Text>
        )}
      </Stack>
    </Stack>
  );
};

export default CustomCategories;
