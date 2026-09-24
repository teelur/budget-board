import { Button } from "@teelur/budget-board-ui";
import { ArrowDownIcon, ArrowUpDownIcon, ArrowUpIcon } from "lucide-react";
import React from "react";

export enum SortDirection {
  None,
  Ascending,
  Decending,
}

interface SortButtonProps {
  label: string;
  sortDirection: SortDirection;
  onClick: () => void;
  [x: string]: any;
}

const SortButton = ({
  label,
  sortDirection,
  onClick,
  ...props
}: SortButtonProps): React.ReactNode => {
  const getSortedIcon = (): React.ReactNode => {
    let sortedIcon = <ArrowUpDownIcon size="1rem" />;
    switch (sortDirection) {
      case SortDirection.Ascending:
        sortedIcon = <ArrowUpIcon size="1rem" />;
        break;
      case SortDirection.Decending:
        sortedIcon = <ArrowDownIcon size="1rem" />;
        break;
      default:
        break;
    }

    return sortedIcon;
  };

  return (
    <Button rightSection={getSortedIcon()} onClick={onClick} {...props}>
      {label}
    </Button>
  );
};

export default SortButton;
