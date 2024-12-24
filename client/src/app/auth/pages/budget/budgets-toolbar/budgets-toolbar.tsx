import { Button } from '@/components/ui/button';
import BudgetsToolcard from './budgets-toolcard';
import { ChevronLeftIcon, ChevronRightIcon } from '@radix-ui/react-icons';
import { cn, getDateFromMonthsAgo, initCurrentMonth, isInArray } from '@/lib/utils';
import React from 'react';
import ResponsiveButton from '@/components/responsive-button';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { BudgetResponse, CashFlowValue, NewBudgetRequest } from '@/types/budget';
import { AuthContext } from '@/components/auth-provider';
import { AxiosError, AxiosResponse } from 'axios';
import { toast } from 'sonner';
import { translateAxiosError } from '@/lib/requests';
import AddButtonPopover from '@/components/add-button-popover';
import AddBudget from '../add-budget';
import { useMeasure } from '@uidotdev/usehooks';

interface BudgetsToolbarProps {
  selectedDates: Date[];
  setSelectedDates: (newDates: Date[]) => void;
  timeToMonthlyTotalsMap: Map<number, number>;
  showCopy: boolean;
  isPending: boolean;
}

const BudgetsToolbar = (props: BudgetsToolbarProps): JSX.Element => {
  const [index, setIndex] = React.useState(0);
  const [selectMultiple, setSelectMultiple] = React.useState(false);

  const [ref, { width }] = useMeasure();

  // Padding of 25 on each side and each card is roughly 70 pixels.
  const dates = Array.from({ length: Math.floor(((width ?? 0) - 72) / 68) }, (_, i) =>
    getDateFromMonthsAgo(i + index)
  );

  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doCopyBudget = useMutation({
    mutationFn: async (newBudgets: NewBudgetRequest[]) =>
      await request({
        url: '/api/budget/addmultiple',
        method: 'POST',
        data: newBudgets,
      }),
    onSuccess: async (_, variables: NewBudgetRequest[]) =>
      await queryClient.invalidateQueries({
        queryKey: ['budgets', variables[0]?.date],
      }),
    onError: (error: AxiosError) => toast.error(translateAxiosError(error)),
  });

  const onCopyBudgets = (): void => {
    const lastMonth = new Date(props.selectedDates[0]);
    lastMonth.setMonth(lastMonth.getMonth() - 1);

    request({
      url: '/api/budget',
      method: 'GET',
      params: { date: lastMonth },
    })
      .then((res: AxiosResponse<any, any>) => {
        const budgets: BudgetResponse[] = res.data;
        if (budgets.length !== 0) {
          const newBudgets: NewBudgetRequest[] = budgets.map((budget) => {
            return {
              date: props.selectedDates[0],
              category: budget.category,
              limit: budget.limit,
            };
          });
          console.log(newBudgets);
          doCopyBudget.mutate(newBudgets);
        } else {
          toast.error('Previous month has no budget!');
        }
      })
      .catch(() => {
        toast.error("There was an error copying the previous month's budget.");
      });
  };

  const handleClick = (date: Date) => {
    if (selectMultiple) {
      // When select multiple is on, we need to add/remove selected dates when clicked.
      if (isInArray(date, props.selectedDates)) {
        // If it is present, then we need to remove it.
        props.setSelectedDates(
          props.selectedDates.filter(
            (selectedDate: Date) => selectedDate.getTime() !== date.getTime()
          )
        );
      } else {
        // If it isn't present, then add to selected.
        props.setSelectedDates([date, ...props.selectedDates]);
      }
    } else {
      // When select multiple is off, we should switch the date when clicked.
      props.setSelectedDates([date]);
    }
  };

  const toggleSelectMultiple = () => {
    if (selectMultiple) {
      // Need to pick the date used for our single date.
      if (props.selectedDates.length === 0) {
        // When nothing is selected, revert back to today.
        props.setSelectedDates([initCurrentMonth()]);
        setIndex(0);
      } else {
        // Otherwise select the most recent selected date.
        props.setSelectedDates([
          new Date(Math.max(...props.selectedDates.map((d) => d.getTime()))),
        ]);
      }
    }

    setSelectMultiple(!selectMultiple);
  };

  const getCashFlowValue = (date: Date): CashFlowValue => {
    const cashFlow = props.timeToMonthlyTotalsMap.get(date.getTime()) ?? 0;
    if (cashFlow > 0) {
      return CashFlowValue.Positive;
    } else if (cashFlow < 0) {
      return CashFlowValue.Negative;
    }
    return CashFlowValue.Neutral;
  };

  return (
    <div ref={ref} className="flex flex-col gap-2">
      <div className="flex flex-row">
        <span className="grow" />
        <Button
          className={cn(
            'w-[125px]',
            selectMultiple ? 'border-success text-success hover:text-success' : ''
          )}
          variant="outline"
          onClick={toggleSelectMultiple}
        >
          Select Multiple
        </Button>
      </div>
      <div className="flex flex-row items-center gap-2">
        <Button
          className="h-[62px] w-8 p-1"
          variant="outline"
          onClick={() => {
            setIndex(index + 1);
          }}
        >
          <ChevronLeftIcon />
        </Button>
        <div className="flex grow flex-row-reverse justify-between">
          {dates.map((date: Date, i: number) => (
            <BudgetsToolcard
              key={i}
              date={date}
              isSelected={isInArray(date, props.selectedDates)}
              isPending={props.isPending}
              cashFlowValue={getCashFlowValue(date)}
              handleClick={handleClick}
            />
          ))}
        </div>
        <Button
          className="h-[62px] w-8 p-1"
          disabled={index === 0}
          variant="outline"
          onClick={() => {
            if (index > 0) setIndex(index - 1);
          }}
        >
          <ChevronRightIcon />
        </Button>
      </div>
      <div
        className={cn('flex flex-row', (selectMultiple || props.isPending) && 'hidden')}
      >
        {props.showCopy && (
          <ResponsiveButton
            className="p-2"
            variant="default"
            loading={doCopyBudget.isPending}
            onClick={onCopyBudgets}
          >
            Copy previous month
          </ResponsiveButton>
        )}
        <span className="grow" />
        <AddButtonPopover>
          <AddBudget date={props.selectedDates[0]} />
        </AddButtonPopover>
      </div>
    </div>
  );
};

export default BudgetsToolbar;
