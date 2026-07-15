import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { PlusIcon } from "lucide-react";
import React from "react";
import { ICategoryNode } from "~/models/category";
import UnbudgetedChildCard from "./UnbudgetedChildCard/UnbudgetedChildCard";
import { roundAwayFromZero } from "~/helpers/utils";
import { uncategorizedTransactionCategory } from "~/models/transaction";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface UnbudgetedCardProps {
  categoryTree: ICategoryNode;
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedCard = (props: UnbudgetedCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale, dayjs } = useLocale();
  const { preferredCurrency } = useUserSettings();
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
          amount={
            props.categoryToTransactionsTotalMap.get(
              subCategory.value.toLocaleLowerCase(),
            )!
          }
          selectedDate={props.selectedDate}
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
            {props.categoryTree.value.length === 0
              ? t(uncategorizedTransactionCategory)
              : props.categoryTree.value}
          </PrimaryText>
          <Group gap="sm">
            <PrimaryText size="1rem" fw={600}>
              {convertNumberToCurrency(
                props.categoryToTransactionsTotalMap.get(
                  props.categoryTree.value.toLocaleLowerCase(),
                ) ?? 0,
                false,
                preferredCurrency,
                SignDisplay.Auto,
                intlLocale,
              )}
            </PrimaryText>
            {props.selectedDate && props.categoryTree.value.length !== 0 && (
              <ActionIcon
                size="sm"
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
                <PlusIcon />
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
