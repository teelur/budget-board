import { Accordion as MantineAccordion, Group, Stack } from "@mantine/core";
import React from "react";
import UnbudgetedCard from "./UnbudgetedCard/UnbudgetedCard";
import { CategoryNode, ICategory, ICategoryNode } from "~/models/category";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface UnbudgetedGroupProps {
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categories: ICategory[];
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedGroup = (props: UnbudgetedGroupProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const total =
    props.categoryTree.reduce((acc, category) => {
      const categoryTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase(),
      );
      return acc + (categoryTotal ? categoryTotal : 0);
    }, 0) + (props.categoryToTransactionsTotalMap.get("") ?? 0);

  const getUnbudgetedCards = (): React.ReactNode[] => {
    const cards: React.ReactNode[] = [];

    if (props.categoryToTransactionsTotalMap.has("")) {
      cards.push(
        <UnbudgetedCard
          key="uncategorized"
          categoryTree={
            new CategoryNode({
              value: "",
              parent: "",
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
    <Accordion defaultValue={[]} elevation={1}>
      <MantineAccordion.Item value="unbudgeted">
        <MantineAccordion.Control>
          <Group justify="space-between" align="center" w="100%" pr="1rem">
            <PrimaryText size="lg">{t("unbudgeted")}</PrimaryText>
            {userSettingsQuery.isPending ? null : (
              <PrimaryText size="lg">
                {convertNumberToCurrency(
                  total,
                  false,
                  userSettingsQuery.data?.currency ?? "USD",
                  SignDisplay.Auto,
                  intlLocale,
                )}
              </PrimaryText>
            )}
          </Group>
        </MantineAccordion.Control>
        <MantineAccordion.Panel>
          <Stack gap="0.5rem">
            {unbudgetedCards.length > 0 ? (
              unbudgetedCards
            ) : (
              <DimmedText size="sm">{t("no_unbudgeted_categories")}</DimmedText>
            )}
          </Stack>
        </MantineAccordion.Panel>
      </MantineAccordion.Item>
    </Accordion>
  );
};

export default UnbudgetedGroup;
