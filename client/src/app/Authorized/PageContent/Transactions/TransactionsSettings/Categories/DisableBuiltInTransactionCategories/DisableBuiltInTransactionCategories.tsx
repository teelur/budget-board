import {
  Button,
  Group,
  Popover as MantinePopover,
  Skeleton,
  Stack,
} from "@mantine/core";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Popover from "~/components/core/Popover/Popover";
import { useTranslation } from "react-i18next";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { defaultGuid } from "~/models/applicationUser";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";
import { areStringsEqual } from "~/helpers/utils";

const DisableBuiltInTransactionCategories = (): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories, customTransactionCategories } =
    useTransactionCategories();
  const { disableBuiltInTransactionCategories } = useUserSettings();
  const transactionsQuery = useTransactionsQuery();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();
  const [isConfirmationOpen, setIsConfirmationOpen] = React.useState(false);

  if (transactionsQuery.isPending) {
    return <Skeleton height={75} radius="md" />;
  }

  const builtInCategoryValues = new Set(
    allTransactionCategories
      .filter(
        (c) =>
          c.id === defaultGuid &&
          !areStringsEqual(c.value, "hide from budgets"),
      )
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

  const hasBlockingReferences =
    transactionsUsingBuiltIn.length > 0 ||
    customCategoriesWithBuiltInParent.length > 0;
  const shouldConfirmDisable =
    !disableBuiltInTransactionCategories && hasBlockingReferences;

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

  const handleToggle = () => {
    updateUserSettingsMutation.mutate({
      disableBuiltInTransactionCategories: !disableBuiltInTransactionCategories,
    } as IUserSettingsUpdateRequest);
  };

  const toggleButton = (
    <Button
      bg={
        disableBuiltInTransactionCategories
          ? "var(--button-color-destructive)"
          : ""
      }
      variant="primary"
      size="xs"
      loading={updateUserSettingsMutation.isPending}
      onClick={
        shouldConfirmDisable ? () => setIsConfirmationOpen(true) : handleToggle
      }
    >
      {disableBuiltInTransactionCategories ? t("disabled") : t("enabled")}
    </Button>
  );

  return (
    <Stack gap="0.25rem">
      <PrimaryText size="sm">
        {t("built_in_transaction_categories")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("disable_built_in_transaction_categories_description")}
      </DimmedText>
      {hasBlockingReferences &&
        blockingReasons.map((reason, i) => (
          <PrimaryText key={i} size="xs">
            {reason}
          </PrimaryText>
        ))}
      {shouldConfirmDisable ? (
        <Popover
          opened={isConfirmationOpen}
          onChange={setIsConfirmationOpen}
          position="bottom-start"
          withArrow
        >
          <MantinePopover.Target>{toggleButton}</MantinePopover.Target>
          <MantinePopover.Dropdown maw={350}>
            <Stack gap="0.5rem">
              <PrimaryText size="xs">
                {t("disable_built_in_transaction_categories_warning")}
              </PrimaryText>
              {blockingReasons.map((reason, i) => (
                <PrimaryText key={i} size="xs">
                  {reason}
                </PrimaryText>
              ))}
              <Group justify="flex-end" gap="0.5rem">
                <Button
                  variant="subtle"
                  size="xs"
                  onClick={() => setIsConfirmationOpen(false)}
                >
                  {t("cancel")}
                </Button>
                <Button
                  color="var(--button-color-destructive)"
                  size="xs"
                  loading={updateUserSettingsMutation.isPending}
                  onClick={() => {
                    setIsConfirmationOpen(false);
                    handleToggle();
                  }}
                >
                  {t("confirm_disable_built_in_transaction_categories")}
                </Button>
              </Group>
            </Stack>
          </MantinePopover.Dropdown>
        </Popover>
      ) : (
        toggleButton
      )}
    </Stack>
  );
};

export default DisableBuiltInTransactionCategories;
