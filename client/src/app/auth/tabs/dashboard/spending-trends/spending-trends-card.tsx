import { Card } from '@/components/ui/card';
import { Separator } from '@radix-ui/react-separator';
import SpendingTrendsChart from './spending-trends-chart';
import React from 'react';
import { AuthContext } from '@/components/auth-provider';
import { useQuery } from '@tanstack/react-query';
import { Transaction } from '@/types/transaction';
import { AxiosResponse } from 'axios';
import { convertNumberToCurrency, getDateFromMonthsAgo } from '@/lib/utils';
import {
  getRollingTotalSpendingForMonth,
  getTransactionsForMonth,
} from '@/lib/transactions';
import { Skeleton } from '@/components/ui/skeleton';

const SpendingTrendsCard = (): JSX.Element => {
  const { request } = React.useContext<any>(AuthContext);
  const transactionsQuery = useQuery({
    queryKey: ['transactions'],
    queryFn: async (): Promise<Transaction[]> => {
      const res: AxiosResponse = await request({
        url: '/api/transaction',
        method: 'GET',
      });

      if (res.status == 200) {
        return res.data;
      }

      return [];
    },
  });

  const getSpendingComparison = (): number => {
    const thisMonthTransactions = getTransactionsForMonth(
      transactionsQuery.data ?? [],
      getDateFromMonthsAgo(0)
    );
    const lastMonthTransactions = getTransactionsForMonth(
      transactionsQuery.data ?? [],
      getDateFromMonthsAgo(1)
    );

    const thisMonthRollingTotal = getRollingTotalSpendingForMonth(thisMonthTransactions);
    const lastMonthRollingTotal = getRollingTotalSpendingForMonth(lastMonthTransactions);

    const today = new Date().getDate();

    return (
      thisMonthRollingTotal[today - 1].amount - lastMonthRollingTotal[today - 1].amount
    );
  };

  const getSpendingComparisonString = (): string => {
    // Need to round this number to the nearest cent
    const spendingComparisonNumber =
      Math.round((getSpendingComparison() + Number.EPSILON) * 100) / 100;

    if (spendingComparisonNumber < 0) {
      return (
        convertNumberToCurrency(Math.abs(spendingComparisonNumber), true) + ' less than'
      );
    } else if (spendingComparisonNumber > 0) {
      return (
        convertNumberToCurrency(Math.abs(spendingComparisonNumber), true) + ' more than'
      );
    }

    return 'the same as';
  };

  if (transactionsQuery.isPending) {
    return (
      <Card>
        <div className="m-3 flex flex-col space-y-3">
          <div className="grid w-full grid-cols-2 items-center">
            <Skeleton className="h-10 w-1/2 max-w-[125px] justify-self-start" />
            <Skeleton className="h-6 w-1/2 max-w-[250px] justify-self-end" />
          </div>
          <Skeleton className="h-[250px] rounded-xl" />
        </div>
      </Card>
    );
  }

  return (
    <Card className="w-full p-6">
      <div className="flex flex-row items-center">
        <span className="w-1/2 text-2xl font-semibold tracking-tight">
          Spending Trends
        </span>
        <span className="w-1/2 text-right tracking-tight">
          You have spent {getSpendingComparisonString()} last month so far.
        </span>
      </div>
      <Separator />
      <div>
        <SpendingTrendsChart
          months={[getDateFromMonthsAgo(0), getDateFromMonthsAgo(1)]}
          transactions={transactionsQuery.data ?? []}
        />
      </div>
    </Card>
  );
};

export default SpendingTrendsCard;
