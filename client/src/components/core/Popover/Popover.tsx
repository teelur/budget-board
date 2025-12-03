import dropdownClasses from "~/styles/Dropdown.module.css";

import {
  Popover as MantinePopover,
  PopoverProps as MantinePopoverProps,
} from "@mantine/core";

export interface PopoverProps extends MantinePopoverProps {
  children: React.ReactNode;
}

const Popover = ({ children, ...props }: PopoverProps): React.ReactNode => {
  return (
    <MantinePopover
      classNames={{
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    >
      {children}
    </MantinePopover>
  );
};

export default Popover;
