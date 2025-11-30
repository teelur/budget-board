import baseClasses from "~/styles/Base.module.css";

import { Select, SelectProps } from "@mantine/core";

export interface BaseSelectProps extends SelectProps {}

const BaseSelect = ({ ...props }: BaseSelectProps): React.ReactNode => {
  return (
    <Select
      classNames={{
        input: baseClasses.input,
      }}
      {...props}
    />
  );
};

export default BaseSelect;
