import { Group, Stack } from "@mantine/core";
import React from "react";
import UnbudgetedCard from "./UnbudgetedCard/UnbudgetedCard";
import {
  CategoryNode,
  CategoryTypes,
  ICategory,
  ICategoryNode,
} from "~/models/category";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface UnbudgetedGroupProps {
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categories: ICategory[];
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
  showUncategorized?: boolean;
}

const UnbudgetedGroup = (props: UnbudgetedGroupProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();

  const total =
    props.categoryTree.reduce((acc, category) => {
      const categoryTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase(),
      );
      return acc + (categoryTotal ? categoryTotal : 0);
    }, 0) +
    (props.showUncategorized
      ? (props.categoryToTransactionsTotalMap.get("") ?? 0)
      : 0);

  const getUnbudgetedCards = (): React.ReactNode[] => {
    const cards: React.ReactNode[] = [];

    if (
      props.showUncategorized &&
      props.categoryToTransactionsTotalMap.has("")
    ) {
      cards.push(
        <UnbudgetedCard
          key="uncategorized"
          categoryTree={
            new CategoryNode({
              value: "",
              parent: "",
              categoryType: CategoryTypes.Expense,
            })
          }
          categoryToTransactionsTotalMap={props.categoryToTransactionsTotalMap}
          selectedDate={props.selectedDate}
          openDetails={props.openDetails}
        />,
      );
    }

    props.categoryTree.forEach((categoryTree) => {
      cards.push(
        <UnbudgetedCard
          key={categoryTree.value}
          categoryTree={categoryTree}
          categoryToTransactionsTotalMap={props.categoryToTransactionsTotalMap}
          selectedDate={props.selectedDate}
          openDetails={props.openDetails}
        />,
      );
    });

    return cards;
  };

  const unbudgetedCards = getUnbudgetedCards();

  return (
    <Accordion p="0.5rem" elevation={1}>
      <Accordion.Item
        defaultOpen={false}
        slim
        title={
          <Group justify="space-between" align="center" w="100%" pr="0.25rem">
            <PrimaryText size="lg">{t("unbudgeted")}</PrimaryText>
            <PrimaryText size="lg">
              {convertNumberToCurrency(
                total,
                false,
                preferredCurrency,
                SignDisplay.Auto,
                intlLocale,
              )}
            </PrimaryText>
          </Group>
        }
      >
        <Stack gap="0.5rem">
          {unbudgetedCards.length > 0 ? (
            unbudgetedCards
          ) : (
            <DimmedText size="sm">{t("no_unbudgeted_categories")}</DimmedText>
          )}
        </Stack>
      </Accordion.Item>
    </Accordion>
  );
};

export default UnbudgetedGroup;
