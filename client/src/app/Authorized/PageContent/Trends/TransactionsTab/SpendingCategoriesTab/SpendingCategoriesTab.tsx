import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { getUniqueYears } from "~/helpers/datetime";
import {
  buildTimeToMonthlyTotalsMap,
  filterHiddenTransactions,
  getTransactionsForMonth,
} from "~/helpers/transactions";
import { Button, Flex, Group, Stack } from "@mantine/core";
import {
  defaultTransactionCategories,
  ITransaction,
} from "~/models/transaction";
import { useQueries, useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import SpendingCategoriesChart from "~/components/Charts/SpendingCategoriesChart/SpendingCategoriesChart";
import { ICategoryResponse } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTranslation } from "react-i18next";

const SpendingCategoriesTab = (): React.ReactNode => {
  const { request } = useAuth();
  const { t } = useTranslation();
  const { dayjs } = useLocale();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().startOf("month").toDate(),
  ]);
  const [showSubcategories, setShowSubcategories] = React.useState(true);
  const monthButtons = [3, 6, 12];

  // Querying by year is the best balance of covering probable dates a user will select,
  // while also not potentially querying for a large amount of data.
  const transactionsQuery = useQueries({
    queries: getUniqueYears(selectedMonths).map((year: number) => ({
      queryKey: ["transactions", { year }],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params: { year },
        });

        if (res.status === 200) {
          return res.data as ITransaction[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

  // We need to filter out the transactions labelled with 'Hide From Budgets'
  const transactionsWithoutHidden = filterHiddenTransactions(
    transactionsQuery.data ?? [],
  );

  const transactionsForSelectedMonths =
    selectedMonths.length > 0
      ? transactionsWithoutHidden.filter((t) =>
          selectedMonths.some(
            (m) => getTransactionsForMonth([t], m).length > 0,
          ),
        )
      : transactionsWithoutHidden;

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? [],
  );

  return (
    <Stack p="0.5rem" gap="1rem">
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        timeToMonthlyTotalsMap={buildTimeToMonthlyTotalsMap(
          selectedMonths,
          transactionsWithoutHidden,
        )}
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
      />
      <Group w="100%" justify="space-between">
        <Button
          size="compact-sm"
          variant={showSubcategories ? "outline" : "subtle"}
          onClick={() => setShowSubcategories((v) => !v)}
        >
          {showSubcategories
            ? t("show_subcategories")
            : t("hide_subcategories")}
        </Button>
        <Group gap="xs">
          {monthButtons.map((months) => (
            <Button
              key={months}
              size="compact-sm"
              variant="light"
              onClick={() => {
                const newMonths: Date[] = [];
                for (let i = 0; i < months; i++) {
                  newMonths.push(
                    dayjs().subtract(i, "month").startOf("month").toDate(),
                  );
                }
                setSelectedMonths(newMonths);
              }}
            >
              {t("last_n_months", { count: months })}
            </Button>
          ))}
          <Button
            size="compact-sm"
            variant="primary"
            onClick={() => setSelectedMonths([])}
          >
            {t("clear_selection")}
          </Button>
        </Group>
      </Group>
      <Flex justify="center">
        <SpendingCategoriesChart
          transactions={transactionsForSelectedMonths}
          categories={transactionCategoriesWithCustom}
          showSubcategories={showSubcategories}
        />
      </Flex>
    </Stack>
  );
};

export default SpendingCategoriesTab;
