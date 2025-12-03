import surfaceClasses from "~/styles/Surface.module.css";

import { Checkbox, CheckboxProps } from "@mantine/core";

export interface SurfaceCheckboxProps extends CheckboxProps {}

const SurfaceCheckbox = ({
  ...props
}: SurfaceCheckboxProps): React.ReactNode => {
  return (
    <Checkbox
      classNames={{
        input: surfaceClasses.input,
      }}
      {...props}
    />
  );
};

export default SurfaceCheckbox;
