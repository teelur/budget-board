import { Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import AddAccountType from "./AddAccountType/AddAccountType";
import CustomAccountTypeCards from "./CustomAccountTypeCards/CustomAccountTypeCards";
import DisableBuiltInAccountTypes from "./DisableBuiltInAccountTypes/DisableBuiltInAccountTypes";

const AccountTypes = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <DisableBuiltInAccountTypes />
      <Stack gap="0.25rem">
        <PrimaryText size="sm">{t("custom_account_types")}</PrimaryText>
        <DimmedText size="xs">
          {t("custom_account_types_description")}
        </DimmedText>
      </Stack>
      <AddAccountType />
      <CustomAccountTypeCards />
    </Stack>
  );
};

export default AccountTypes;
