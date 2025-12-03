import surfaceClasses from "~/styles/Surface.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import { Select, SelectProps } from "@mantine/core";

export interface SurfaceSelectProps extends SelectProps {}

const SurfaceSelect = ({ ...props }: SurfaceSelectProps): React.ReactNode => {
  return (
    <Select
      classNames={{
        input: surfaceClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default SurfaceSelect;
