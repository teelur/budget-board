import React from 'react';
import { BudgetGroup, getBudgetsForMonth, getBudgetsForGroup } from '@/lib/budgets';
import { type Budget } from '@/types/budget';
import BudgetTotalCard from './budget-total-card';
import { initCurrentMonth } from '@/lib/utils';
import { type Transaction } from '@/types/transaction';
import Unbudgets from './unbudgets';
import BudgetsToolbar from './budgets-toolbar';
import { AuthContext } from '@/components/auth-provider';
import { useQuery } from '@tanstack/react-query';
import BudgetCardsGroup from './budget-cards-group/budget-cards-group';
import { AxiosResponse } from 'axios';

const Budgets = (): JSX.Element => {
  const [date, setDate] = React.useState<Date>(initCurrentMonth());

  const { request } = React.useContext<any>(AuthContext);

  const budgetsQuery = useQuery({
    queryKey: ['budgets', date],
    queryFn: async (): Promise<Budget[]> => {
      const res: AxiosResponse = await request({
        url: '/api/budget',
        method: 'GET',
        params: { date },
      });

      if (res.status == 200) {
        return res.data;
      }

      return [];
    },
  });

  const transactionsForMonthQuery = useQuery({
    queryKey: ['transactions', { month: date.getMonth(), year: date.getUTCFullYear() }],
    queryFn: async (): Promise<Transaction[]> => {
      const res: AxiosResponse = await request({
        url: '/api/transaction',
        method: 'GET',
        params: { date: date },
      });

      if (res.status == 200) {
        return res.data;
      }

      return [];
    },
  });

  return (
    <div className="flex w-full max-w-screen-2xl flex-col justify-center gap-2 lg:grid lg:grid-flow-col lg:grid-cols-12">
      <div className="space-y-2 lg:col-span-9">
        <BudgetsToolbar
          budgets={budgetsQuery.data ?? []}
          date={date}
          isPending={budgetsQuery.isPending}
          setDate={setDate}
        />
        <div className="space-y-10">
          <BudgetCardsGroup
            header={'Income'}
            budgetData={getBudgetsForGroup(budgetsQuery.data, BudgetGroup.Income)}
            transactionsData={transactionsForMonthQuery.data ?? []}
            isPending={budgetsQuery.isPending || transactionsForMonthQuery.isPending}
          />
          <BudgetCardsGroup
            header={'Spending'}
            budgetData={getBudgetsForGroup(budgetsQuery.data, BudgetGroup.Spending)}
            transactionsData={transactionsForMonthQuery.data ?? []}
            isPending={budgetsQuery.isPending || transactionsForMonthQuery.isPending}
          />
        </div>
        <Unbudgets
          transactions={transactionsForMonthQuery.data ?? []}
          budgets={budgetsQuery.data ?? []}
          isPending={budgetsQuery.isPending || transactionsForMonthQuery.isPending}
        />
      </div>
      <div className="h-96 lg:col-span-3">
        <BudgetTotalCard
          budgetData={getBudgetsForMonth(budgetsQuery.data ?? [], date)}
          transactionData={transactionsForMonthQuery.data ?? []}
          isPending={budgetsQuery.isPending || transactionsForMonthQuery.isPending}
        />
      </div>
    </div>
  );
};

export default Budgets;
