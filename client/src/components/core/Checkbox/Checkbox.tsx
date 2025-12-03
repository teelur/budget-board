import { CheckboxProps as MantineCheckboxProps } from "@mantine/core";
import BaseCheckbox from "./BaseCheckbox/BaseCheckbox";
import SurfaceCheckbox from "./SurfaceCheckbox/SurfaceCheckbox";

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
      throw new Error("Elevation level not implemented for Checkbox");
    default:
      return <BaseCheckbox {...props} />;
  }
};
export default Checkbox;
