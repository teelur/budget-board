import { Stack } from "@mantine/core";
import React from "react";
import DisableBuiltInTransactionCategories from "./DisableBuiltInTransactionCategories/DisableBuiltInTransactionCategories";
import CustomCategories from "./CustomCategories/CustomCategories";

const Categories = (): React.ReactNode => {
  return (
    <Stack gap="0.5rem">
      <DisableBuiltInTransactionCategories />
      <CustomCategories />
    </Stack>
  );
};

export default Categories;
