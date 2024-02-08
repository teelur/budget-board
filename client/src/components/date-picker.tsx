import { CalendarIcon } from '@radix-ui/react-icons';
import { Button } from './ui/button';
import { Popover, PopoverContent, PopoverTrigger } from './ui/popover';
import { Calendar } from './ui/calendar';
import { cn } from '@/lib/utils';
import { format } from 'date-fns';

interface DatePickerProps {
  value: Date;
  setDatePick: (day: Date) => void;
}

const DatePicker = ({ value, setDatePick }: DatePickerProps): JSX.Element => {
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant={'outline'}
          className={cn(
            'min-w-[200px] max-w-full justify-start text-left font-normal',
            value == null && 'text-muted-foreground'
          )}
        >
          <CalendarIcon className="mr-2 h-4 w-4" />
          {value != null ? format(value, 'PPP') : <span>Pick a date</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0">
        <Calendar
          mode="single"
          selected={value}
          defaultMonth={value}
          onDayClick={setDatePick}
          initialFocus
        />
      </PopoverContent>
    </Popover>
  );
};

export default DatePicker;
