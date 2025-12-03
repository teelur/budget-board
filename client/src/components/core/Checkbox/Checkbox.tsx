import { CheckboxProps as MantineCheckboxProps } from "@mantine/core";
import BaseCheckbox from "./BaseCheckbox/BaseCheckbox";
import SurfaceCheckbox from "./SurfaceCheckbox/SurfaceCheckbox";
import ElevatedCheckbox from "./ElevatedCheckbox/ElevatedCheckbox";

export interface CheckboxProps extends MantineCheckboxProps {
  elevation?: number;
}

const Checkbox = ({
  elevation = 0,
  ...props
}: CheckboxProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseCheckbox {...props} />;
    case 1:
      return <SurfaceCheckbox {...props} />;
    case 2:
      return <ElevatedCheckbox {...props} />;
    default:
      throw new Error("Invalid elevation level for Checkbox");
  }
};
export default Checkbox;
