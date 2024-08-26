import { Button } from '@/components/ui/button';
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { cn } from '@/lib/utils';
import { Check, ChevronsUpDown } from 'lucide-react';
import React from 'react';
import CommandSubcategory from './command-subcategory';
import { ICategoryNode } from '@/types/category';

interface CategoryInputProps {
  initialValue: string;
  categoriesTree: ICategoryNode[];
  onSelectChange: (category: string) => void;
}

const CategoryInput = (props: CategoryInputProps): JSX.Element => {
  const [open, setOpen] = React.useState(false);
  const [value, setValue] = React.useState(props.initialValue);

  return (
    <Popover open={open} onOpenChange={setOpen} modal>
      <PopoverTrigger asChild>
        <Button
          variant="dropdown"
          role="combobox"
          aria-expanded={open}
          className="min-w-[50px] max-w-full justify-between"
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          {value.length > 0 ? value : 'Select category...'}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[225px] p-0">
        <Command>
          <CommandInput
            placeholder="Search categories"
            onClick={(e) => {
              e.stopPropagation();
            }}
          />
          <CommandList>
            <CommandEmpty>No categories found.</CommandEmpty>
            {props.categoriesTree.map((category: ICategoryNode) => (
              <CommandGroup
                key={category.value}
                onClick={(e) => {
                  e.stopPropagation();
                }}
              >
                <CommandItem
                  className="font-bold"
                  value={category.value}
                  onSelect={(currentValue) => {
                    setValue(currentValue === value ? '' : currentValue);
                    props.onSelectChange(currentValue === value ? '' : currentValue);
                    setOpen(false);
                  }}
                >
                  <Check
                    className={cn(
                      'mr-2 h-4 w-4',
                      value === category.value ? 'opacity-100' : 'opacity-0'
                    )}
                  />
                  {category.value}
                </CommandItem>
                <CommandSubcategory
                  category={category}
                  initialValue={value}
                  updateValue={(currentValue) => {
                    setValue(currentValue === value ? '' : currentValue);
                    props.onSelectChange(currentValue === value ? '' : currentValue);
                    setOpen(false);
                  }}
                />
              </CommandGroup>
            ))}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
};

export default CategoryInput;
