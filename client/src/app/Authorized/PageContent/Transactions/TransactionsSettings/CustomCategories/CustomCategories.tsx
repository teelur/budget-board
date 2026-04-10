import { Flex, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import AddCategory from "./AddCategory/AddCategory";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ICategoryResponse } from "~/models/category";
import CustomCategoryCard from "./CustomCategoryCard/CustomCategoryCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DisableBuiltInTransactionCategories from "./DisableBuiltInTransactionCategories/DisableBuiltInTransactionCategories";

const CustomCategories = (): React.ReactNode => {
  const { t } = useTranslation();
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

  const getCustomCategoryContent = (): React.ReactNode => {
    if (transactionCategoriesQuery.isPending) {
      return <Skeleton height={46} radius="md" />;
    }

    if ((transactionCategoriesQuery.data ?? []).length === 0) {
      return (
        <Group justify="center" p="1rem">
          <DimmedText size="sm">{t("no_custom_categories")}</DimmedText>
        </Group>
      );
    }

    return transactionCategoriesQuery.data?.map(
      (category: ICategoryResponse) => (
        <CustomCategoryCard key={category.id} category={category} />
      ),
    );
  };

  return (
    <Stack gap="0.5rem">
      <DisableBuiltInTransactionCategories />
      <DimmedText size="sm">{t("custom_categories_description")}</DimmedText>
      <AddCategory />
      <Stack gap="0.25rem">
        <Group px="0.5rem" justify="space-between">
          <Flex w="40%">
            <PrimaryText>{t("name")}</PrimaryText>
          </Flex>
          <Flex w="40%">
            <PrimaryText>{t("parent")}</PrimaryText>
          </Flex>
          <Flex justify="flex-end" flex="1 1 auto">
            <Flex />
          </Flex>
        </Group>
        {getCustomCategoryContent()}
      </Stack>
    </Stack>
  );
};

export default CustomCategories;
