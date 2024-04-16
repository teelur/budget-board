import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Budget } from '@/types/budget';
import { Transaction } from '@/types/transaction';
import UnbudgetCard from './unbudget-card';

interface Unbudget {
  category: string;
  amount: number;
}

const getUnbudgetedTransactions = (budgets: Budget[], transactions: Transaction[]): Unbudget[] => {
  if (budgets == null || transactions == null) return [];

  // This creates an object that maps category => array of amounts
  const groupedTransactions: [string, number[]][] = Object.entries(
    transactions.reduce((result: any, item: Transaction) => {
      (result[item['category']] = result[item['category']] || []).push(item.amount);
      return result;
    }, {})
  );

  const filteredGroupedTransactions = groupedTransactions.filter((t) => {
    return !budgets.some(({ category }) => category === t[0]);
  });

  const unbudgetedTransactions: Unbudget[] = [];
  filteredGroupedTransactions.forEach((element) => {
    unbudgetedTransactions.push({
      category: element[0],
      amount: element[1].reduce((a, b) => {
        return a + b;
      }),
    });
  });

  return unbudgetedTransactions;
};

interface UnbudgetProps {
  transactions: Transaction[];
  budgets: Budget[];
}

const Unbudgets = (props: UnbudgetProps): JSX.Element => {
  return (
    <Accordion type="single" collapsible className="w-full">
      <AccordionItem value="item-1">
        <AccordionTrigger>
          <div className="flex w-full flex-row pl-3">
            <div className="w-1/2 text-left text-lg font-semibold tracking-tight">Other</div>
            <div className="w-1/2">
              <div className="w-1/3 text-center text-lg font-semibold tracking-tight">
                $
                {getUnbudgetedTransactions(props.budgets, props.transactions)
                  .reduce((a: number, b: Unbudget) => {
                    return a + b.amount;
                  }, 0)
                  .toFixed()}
              </div>
            </div>
          </div>
        </AccordionTrigger>
        <AccordionContent className="space-y-1">
          {getUnbudgetedTransactions(props.budgets, props.transactions).map(
            (unbudget: Unbudget) => (
              <UnbudgetCard
                key={unbudget.category}
                name={unbudget.category}
                amount={unbudget.amount.toFixed()}
              />
            )
          )}
        </AccordionContent>
      </AccordionItem>
    </Accordion>
  );
};

export default Unbudgets;
