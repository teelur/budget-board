import { Button } from '@/components/ui/button';
import { getMonthAndYearDateString } from '@/lib/utils';
import { ChevronLeftIcon, ChevronRightIcon } from 'lucide-react';
import React from 'react';

interface MonthIteratorProps {
  date: Date;
  setDate: (date: Date) => void;
}

const MonthIterator = ({ date, setDate }: MonthIteratorProps): JSX.Element => {
  const [displayDate, setDisplayDate] = React.useState<string>(
    getMonthAndYearDateString(date)
  );

  const iterateDate = (iterate: number): void => {
    const newDate = new Date(date);
    newDate.setMonth(newDate.getMonth() + iterate);
    setDate(newDate);
  };

  React.useEffect(() => {
    setDisplayDate(getMonthAndYearDateString(date));
  }, [date]);

  return (
    <div className="flex flex-row items-center justify-center space-x-2">
      <Button
        className="h-8 w-8 p-1"
        variant="ghost"
        onClick={() => {
          iterateDate(-1);
        }}
      >
        <ChevronLeftIcon />
      </Button>
      <div className="w-[130px] text-nowrap text-center text-lg font-semibold">
        {displayDate}
      </div>
      <Button
        className="h-8 w-8 p-1"
        variant="ghost"
        onClick={() => {
          iterateDate(1);
        }}
      >
        <ChevronRightIcon />
      </Button>
    </div>
  );
};

export default MonthIterator;
