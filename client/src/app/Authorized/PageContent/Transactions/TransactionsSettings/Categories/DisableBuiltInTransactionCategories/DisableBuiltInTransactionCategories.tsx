import {
  Group,
  Popover as MantinePopover,
  Skeleton,
  Stack,
} from "@mantine/core";
import { BodyText, Button } from "@teelur/budget-board-ui";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
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
  const transactionsQuery = useTransactionsQuery({
    includeHiddenCategory: true,
    includeHiddenAccounts: true,
  });
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
      variant="filled"
      color={disableBuiltInTransactionCategories ? "error" : "primary"}
      size="compact-sm"
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
      <Stack gap={0}>
        <BodyText tone="primary" size="md" fw={500}>
          {t("built_in_transaction_categories")}
        </BodyText>
        <BodyText tone="secondary" size="sm">
          {t("disable_built_in_transaction_categories_description")}
        </BodyText>
        {hasBlockingReferences &&
          blockingReasons.map((reason, i) => (
            <BodyText tone="primary" size="xs" fw={500} key={i}>
              {reason}
            </BodyText>
          ))}
      </Stack>
      {shouldConfirmDisable ? (
        <Popover
          opened={isConfirmationOpen}
          onChange={setIsConfirmationOpen}
          position="bottom-start"
          withArrow
        >
          <MantinePopover.Target>{toggleButton}</MantinePopover.Target>
          <MantinePopover.Dropdown maw={350}>
            <Stack gap="0.25rem">
              <BodyText tone="primary" size="xs">
                {t("disable_built_in_transaction_categories_warning")}
              </BodyText>
              {blockingReasons.map((reason, i) => (
                <BodyText tone="primary" size="xs" fw={500} key={i}>
                  {reason}
                </BodyText>
              ))}
              <Group justify="flex-end" gap="0.5rem">
                <Button
                  variant="filled"
                  color="neutral"
                  size="xs"
                  onClick={() => setIsConfirmationOpen(false)}
                >
                  {t("cancel")}
                </Button>
                <Button
                  variant="filled"
                  color="error"
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
