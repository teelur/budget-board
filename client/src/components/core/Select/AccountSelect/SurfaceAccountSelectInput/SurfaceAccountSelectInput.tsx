import surfaceClasses from "~/styles/Surface.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AccountSelectInputBase, {
  AccountSelectInputBaseProps,
} from "../AccountSelectInputBase/AccountSelectInputBase";

const SurfaceAccountSelectInput = ({
  ...props
}: AccountSelectInputBaseProps): React.ReactNode => {
  return (
    <AccountSelectInputBase
      classNames={{
        input: surfaceClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default SurfaceAccountSelectInput;
