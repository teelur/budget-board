import React from 'react';
import { BudgetGroup, getBudgetsForMonth, getBudgetsForGroup } from '@/lib/budgets';
import { getTransactionsForMonth } from '@/lib/transactions';
import { type Budget } from '@/types/budget';
import BudgetTotalCard from './budget-total-card';
import { initMonth } from '@/lib/utils';
import { type Transaction } from '@/types/transaction';
import Unbudgets from './unbudgets';
import BudgetsToolbar from './budgets-toolbar';
import { AuthContext } from '@/components/auth-provider';
import { useQuery } from '@tanstack/react-query';
import BudgetCardsGroup from './budget-cards-group/budget-cards-group';

const Budgets = (): JSX.Element => {
  const [date, setDate] = React.useState<Date>(initMonth());

  const { request } = React.useContext<any>(AuthContext);

  const budgetsQuery = useQuery({
    queryKey: ['budgets', { date }],
    queryFn: async () =>
      await request({
        url: '/api/budget',
        method: 'GET',
        params: { date },
      }),
  });

  const transactionsQuery = useQuery({
    queryKey: ['transactions'],
    queryFn: async () =>
      await request({
        url: '/api/transaction',
        method: 'GET',
      }),
  });

  return (
    <div className="flex w-full max-w-screen-2xl flex-col justify-center gap-2 lg:grid lg:grid-flow-col lg:grid-cols-12">
      <div className="space-y-2 lg:col-span-9">
        <BudgetsToolbar
          budgets={budgetsQuery.data?.data ?? []}
          date={date}
          isPending={budgetsQuery.isPending}
          setDate={setDate}
        />
        <div className="space-y-10">
          <BudgetCardsGroup
            header={'Income'}
              budgetData={getBudgetsForGroup(
                budgetsQuery.data?.data as Budget[],
                BudgetGroup.Income
              )}
              transactionsData={getTransactionsForMonth(
                (transactionsQuery.data?.data as Transaction[]) ?? [],
                date
              )}
              isPending={budgetsQuery.isPending || transactionsQuery.isPending}
            />
          <BudgetCardsGroup
            header={'Spending'}
              budgetData={getBudgetsForGroup(
                budgetsQuery.data?.data as Budget[],
                BudgetGroup.Spending
              )}
              transactionsData={getTransactionsForMonth(
                (transactionsQuery.data?.data as Transaction[]) ?? [],
                date
              )}
              isPending={budgetsQuery.isPending || transactionsQuery.isPending}
            />
        </div>
        <Unbudgets
          transactions={getTransactionsForMonth(
            (transactionsQuery.data?.data as Transaction[]) ?? [],
            date
          )}
          budgets={(budgetsQuery.data?.data as Budget[]) ?? []}
          isPending={budgetsQuery.isPending || transactionsQuery.isPending}
        />
      </div>
      <div className="h-96 lg:col-span-3">
        <BudgetTotalCard
          budgetData={getBudgetsForMonth(
            (budgetsQuery.data?.data as Budget[]) ?? [],
            date
          )}
          transactionData={getTransactionsForMonth(
            (transactionsQuery.data?.data as Transaction[]) ?? [],
            date
          )}
        />
      </div>
    </div>
  );
};

export default Budgets;
