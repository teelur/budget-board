import { Stack } from "@mantine/core";
import { BodyText } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import AddCategory from "./AddCategory/AddCategory";
import CustomCategoryCards from "./CustomCategoryCards/CustomCategoryCards";

const CustomCategories = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <Stack gap={0}>
        <BodyText tone="primary" size="md" fw={500}>
          {t("custom_categories")}
        </BodyText>
        <BodyText tone="secondary" size="sm">
          {t("custom_categories_description")}
        </BodyText>
      </Stack>
      <AddCategory />
      <CustomCategoryCards />
    </Stack>
  );
};

export default CustomCategories;
