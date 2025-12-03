import baseClasses from "~/styles/Base.module.css";

import { Checkbox, CheckboxProps } from "@mantine/core";

export interface BaseCheckboxProps extends CheckboxProps {}

const BaseCheckbox = ({ ...props }: BaseCheckboxProps): React.ReactNode => {
  return (
    <Checkbox
      classNames={{
        input: baseClasses.input,
      }}
      {...props}
    />
  );
};

export default BaseCheckbox;
