import classes from "./UnbudgetedGroup.module.css";

import { Group, Stack } from "@mantine/core";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { SignDisplay } from "~/helpers/currency";
import React from "react";
import UnbudgetedCard from "./UnbudgetedCard/UnbudgetedCard";
import { CategoryNode, CategoryTypes, ICategoryNode } from "~/models/category";
import { useTranslation } from "react-i18next";
import {
  AmountText,
  BodyText,
  CollapsibleCard,
  HeadingText,
} from "@teelur/budget-board-ui";

interface UnbudgetedGroupProps {
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
  showUncategorized?: boolean;
}

const UnbudgetedGroup = (props: UnbudgetedGroupProps): React.ReactNode => {
  const { t } = useTranslation();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const { preferredCurrency } = useUserSettings();
  const { intlLocale } = useLocale();

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
    <CollapsibleCard
      title={
        <Group justify="space-between" align="center" w="100%" pr="0.25rem">
          <HeadingText tone="primary" className={classes.title}>
            {t("unbudgeted")}
          </HeadingText>
          <AmountText
            amount={total}
            size="lg"
            isSensitive={isPrivacyModeEnabled}
            currency={preferredCurrency}
            decimalPlaces={0}
            locale={intlLocale}
            signDisplay={SignDisplay.Auto}
          />
        </Group>
      }
      defaultExpanded={false}
    >
      <Stack gap="0.5rem">
        {unbudgetedCards.length > 0 ? (
          unbudgetedCards
        ) : (
          <BodyText tone="muted" size="sm" ta="center">
            {t("no_unbudgeted_categories")}
          </BodyText>
        )}
      </Stack>
    </CollapsibleCard>
  );
};

export default UnbudgetedGroup;
