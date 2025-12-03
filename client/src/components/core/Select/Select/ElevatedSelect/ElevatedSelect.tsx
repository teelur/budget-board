import elevatedClasses from "~/styles/Elevated.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import { Select, SelectProps } from "@mantine/core";

export interface ElevatedSelectProps extends SelectProps {}

const ElevatedSelect = ({ ...props }: ElevatedSelectProps): React.ReactNode => {
  return (
    <Select
      classNames={{
        input: elevatedClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default ElevatedSelect;
