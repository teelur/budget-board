import surfaceClasses from "~/styles/Surface.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AccountMultiSelectInputBase, {
  AccountMultiSelectInputBaseProps,
} from "../AccountMultiSelectInputBase/AccountMultiSelectInputBase";

const SurfaceAccountMultiSelectInput = ({
  ...props
}: AccountMultiSelectInputBaseProps): React.ReactNode => {
  return (
    <AccountMultiSelectInputBase
      classNames={{
        input: surfaceClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default SurfaceAccountMultiSelectInput;
