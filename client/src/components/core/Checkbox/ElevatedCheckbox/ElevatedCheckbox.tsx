import elevatedClasses from "~/styles/Elevated.module.css";

import { Checkbox, CheckboxProps } from "@mantine/core";

export interface ElevatedCheckboxProps extends CheckboxProps {}

const ElevatedCheckbox = ({
  ...props
}: ElevatedCheckboxProps): React.ReactNode => {
  return (
    <Checkbox
      classNames={{
        input: elevatedClasses.input,
      }}
      {...props}
    />
  );
};

export default ElevatedCheckbox;
