import { Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import AddAccountType from "./AddAccountType/AddAccountType";
import CustomAccountTypeCards from "./CustomAccountTypeCards/CustomAccountTypeCards";
import DisableBuiltInAccountTypes from "./DisableBuiltInAccountTypes/DisableBuiltInAccountTypes";
import { BodyText } from "@teelur/budget-board-ui";

const AccountTypes = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <DisableBuiltInAccountTypes />
      <Stack gap={0}>
        <BodyText tone="heading" size="sm" fw={500}>
          {t("custom_account_types")}
        </BodyText>
        <BodyText tone="secondary" size="xs">
          {t("custom_account_types_description")}
        </BodyText>
      </Stack>
      <AddAccountType />
      <CustomAccountTypeCards />
    </Stack>
  );
};

export default AccountTypes;
