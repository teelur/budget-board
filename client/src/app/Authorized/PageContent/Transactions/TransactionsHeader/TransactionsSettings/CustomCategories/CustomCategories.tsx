import { Flex, Group, Stack } from "@mantine/core";
import React from "react";
import AddCategory from "./AddCategory/AddCategory";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ICategoryResponse } from "~/models/category";
import CustomCategoryCard from "./CustomCategoryCard/CustomCategoryCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

const CustomCategories = (): React.ReactNode => {
  const { request } = useAuth();

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
      <DimmedText size="sm">
        Create custom categories to organize your transactions.
      </DimmedText>
      <AddCategory />
      <Stack gap="0.25rem">
        <Group px="0.5rem" justify="space-between">
          <Flex w="40%">
            <PrimaryText>Name</PrimaryText>
          </Flex>
          <Flex w="40%">
            <PrimaryText>Parent</PrimaryText>
          </Flex>
          <Flex justify="flex-end" flex="1 1 auto">
            <Flex />
          </Flex>
        </Group>
        {(transactionCategoriesQuery.data ?? []).length > 0 ? (
          transactionCategoriesQuery.data?.map(
            (category: ICategoryResponse) => (
              <CustomCategoryCard key={category.id} category={category} />
            )
          )
        ) : (
          <Group justify="center">
            <DimmedText size="sm">No custom categories.</DimmedText>
          </Group>
        )}
      </Stack>
    </Stack>
  );
};

export default CustomCategories;
