import { SelectProps as MantineSelectProps } from "@mantine/core";
import BaseSelect from "./BaseSelect/BaseSelect";
import SurfaceSelect from "./SurfaceSelect/SurfaceSelect";
import ElevatedSelect from "./ElevatedSelect/ElevatedSelect";

export interface SelectProps extends MantineSelectProps {
  elevation?: number;
}

const Select = ({ elevation = 0, ...props }: SelectProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseSelect {...props} />;
    case 1:
      return <SurfaceSelect {...props} />;
    case 2:
      return <ElevatedSelect {...props} />;
    default:
      throw new Error("Invalid elevation level for Select");
  }
};

export default Select;
