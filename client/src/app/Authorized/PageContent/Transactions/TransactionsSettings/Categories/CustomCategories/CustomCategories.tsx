import { Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import AddCategory from "./AddCategory/AddCategory";
import CustomCategoryCards from "./CustomCategoryCards/CustomCategoryCards";

const CustomCategories = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <Stack gap="0.25rem">
        <PrimaryText size="sm">{t("custom_categories")}</PrimaryText>
        <DimmedText size="xs">{t("custom_categories_description")}</DimmedText>
      </Stack>
      <AddCategory />
      <CustomCategoryCards />
    </Stack>
  );
};

export default CustomCategories;
