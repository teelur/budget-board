import { Group, LoadingOverlay, Stack } from "@mantine/core";
import { ActionIcon, AmountText } from "@teelur/budget-board-ui";
import { PlusIcon } from "lucide-react";
import React from "react";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { ICategoryNode } from "~/models/category";
import UnbudgetedChildCard from "./UnbudgetedChildCard/UnbudgetedChildCard";
import { roundAwayFromZero } from "~/helpers/utils";
import { uncategorizedTransactionCategory } from "~/models/transaction";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";
import { getCategoryIcon } from "~/helpers/category";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { SignDisplay } from "~/helpers/currency";

interface UnbudgetedCardProps {
  categoryTree: ICategoryNode;
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedCard = (props: UnbudgetedCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, intlLocale } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { preferredCurrency } = useUserSettings();
  const { allTransactionCategories } = useTransactionCategories();
  const createBudgetMutation = useCreateBudgetMutation();

  if (
    roundAwayFromZero(
      props.categoryToTransactionsTotalMap.get(
        props.categoryTree.value.toLocaleLowerCase(),
      ) ?? 0,
    ) === 0
  ) {
    return null;
  }

  const categoryIcon = getCategoryIcon(
    props.categoryTree.value,
    allTransactionCategories,
  );

  const getUnbudgetedChildCards = (): React.ReactNode => {
    if (props.categoryTree.subCategories.length === 0) {
      return null;
    }

    const childCards: React.ReactNode[] = [];

    props.categoryTree.subCategories.forEach((subCategory) => {
      if (
        !props.categoryToTransactionsTotalMap.has(
          subCategory.value.toLocaleLowerCase(),
        )
      ) {
        return;
      }
      childCards.push(
        <UnbudgetedChildCard
          key={subCategory.value}
          category={subCategory.value}
          amount={props.categoryToTransactionsTotalMap.get(
            subCategory.value.toLocaleLowerCase(),
          )!}
          selectedDate={props.selectedDate}
          icon={getCategoryIcon(subCategory.value, allTransactionCategories)}
          openDetails={props.openDetails}
        />,
      );
    });

    if (childCards.length === 0) {
      return null;
    }

    return <Stack gap="0.5rem">{childCards}</Stack>;
  };

  return (
    <Stack gap="0.5rem" w="100%">
      <Card
        onClick={() => {
          if (props.selectedDate) {
            props.openDetails(props.categoryTree.value, props.selectedDate);
          }
        }}
        p="0.25rem"
        hoverEffect
        elevation={2}
      >
        <LoadingOverlay visible={createBudgetMutation.isPending} />
        <Group w="100%" justify="space-between">
          <PrimaryText size="md" fw={600}>
            {categoryIcon.length > 0 ? `${categoryIcon} ` : ""}
            {props.categoryTree.value.length === 0
              ? t(uncategorizedTransactionCategory)
              : props.categoryTree.value}
          </PrimaryText>
          <Group gap="sm">
            <AmountText
              amount={
                props.categoryToTransactionsTotalMap.get(
                  props.categoryTree.value.toLocaleLowerCase(),
                ) ?? 0
              }
              disableStatusColor
              size="md"
              isSensitive={isPrivacyModeEnabled}
              currency={preferredCurrency}
              decimalPlaces={0}
              locale={intlLocale}
              signDisplay={SignDisplay.Auto}
            />
            {props.selectedDate && props.categoryTree.value.length !== 0 && (
              <ActionIcon
                variant="filled"
                color="primary"
                size="compact-xs"
                onClick={(event) => {
                  event.stopPropagation();
                  createBudgetMutation.mutate([
                    {
                      month: dayjs(props.selectedDate!).format("YYYY-MM-DD"),
                      category: props.categoryTree.value,
                      limit: Math.round(
                        Math.abs(
                          props.categoryToTransactionsTotalMap.get(
                            props.categoryTree.value.toLocaleLowerCase(),
                          ) ?? 0,
                        ),
                      ),
                    },
                  ]);
                }}
              >
                <PlusIcon size={20} />
              </ActionIcon>
            )}
          </Group>
        </Group>
      </Card>
      {getUnbudgetedChildCards()}
    </Stack>
  );
};

export default UnbudgetedCard;
