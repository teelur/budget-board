import { SelectProps as MantineSelectProps } from "@mantine/core";
import BaseSelect from "./BaseSelect/BaseSelect";

export interface SelectProps extends MantineSelectProps {
  elevation?: number;
}

const Select = ({ elevation, ...props }: SelectProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseSelect {...props} />;
    case 1:
      throw new Error("SurfaceSelect not implemented");
    case 2:
      throw new Error("ElevatedSelect not implemented");
    default:
      return <BaseSelect {...props} />;
  }
};

export default Select;
