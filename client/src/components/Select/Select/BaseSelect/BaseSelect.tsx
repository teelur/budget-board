import baseClasses from "~/styles/Base.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import { Select, SelectProps } from "@mantine/core";

export interface BaseSelectProps extends SelectProps {}

const BaseSelect = ({ ...props }: BaseSelectProps): React.ReactNode => {
  return (
    <Select
      classNames={{
        input: baseClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default BaseSelect;
