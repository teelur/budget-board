import { Button, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { defaultGuid } from "~/models/applicationUser";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const DisableBuiltInTransactionCategories = (): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories, customTransactionCategories } =
    useTransactionCategories();
  const { disableBuiltInTransactionCategories } = useUserSettings();
  const transactionsQuery = useTransactionsQuery();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  if (transactionsQuery.isPending) {
    return <Skeleton height={75} radius="md" />;
  }

  const builtInCategoryValues = new Set(
    allTransactionCategories
      .filter((c) => c.id === defaultGuid)
      .map((c) => c.value.toLowerCase()),
  );

  const transactionsUsingBuiltIn = (transactionsQuery.data ?? []).filter(
    (tx) =>
      (tx.category != null &&
        builtInCategoryValues.has(tx.category.toLowerCase())) ||
      (tx.subcategory != null &&
        builtInCategoryValues.has(tx.subcategory.toLowerCase())),
  );

  const customCategoriesWithBuiltInParent = customTransactionCategories.filter(
    (c) => c.parent !== "" && builtInCategoryValues.has(c.parent.toLowerCase()),
  );

  const canDisable =
    transactionsUsingBuiltIn.length === 0 &&
    customCategoriesWithBuiltInParent.length === 0;

  const blockingReasons: string[] = [];
  if (transactionsUsingBuiltIn.length > 0) {
    blockingReasons.push(
      t("disable_built_in_transaction_categories_blocked_transactions", {
        count: transactionsUsingBuiltIn.length,
      }),
    );
  }
  if (customCategoriesWithBuiltInParent.length > 0) {
    blockingReasons.push(
      t("disable_built_in_transaction_categories_blocked_custom_categories", {
        count: customCategoriesWithBuiltInParent.length,
      }),
    );
  }

  return (
    <Stack gap="0.25rem">
      <PrimaryText size="sm">
        {t("built_in_transaction_categories")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("disable_built_in_transaction_categories_description")}
      </DimmedText>
      {!canDisable &&
        blockingReasons.map((reason, i) => (
          <PrimaryText key={i} size="xs">
            {reason}
          </PrimaryText>
        ))}
      <Button
        bg={
          disableBuiltInTransactionCategories
            ? "var(--button-color-destructive)"
            : ""
        }
        variant="primary"
        size="xs"
        disabled={!disableBuiltInTransactionCategories && !canDisable}
        loading={updateUserSettingsMutation.isPending}
        onClick={() => {
          updateUserSettingsMutation.mutate({
            disableBuiltInTransactionCategories:
              !disableBuiltInTransactionCategories,
          } as IUserSettingsUpdateRequest);
        }}
      >
        {disableBuiltInTransactionCategories ? t("disabled") : t("enabled")}
      </Button>
    </Stack>
  );
};

export default DisableBuiltInTransactionCategories;
