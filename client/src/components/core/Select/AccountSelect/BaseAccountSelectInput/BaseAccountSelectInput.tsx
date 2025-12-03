import baseClasses from "~/styles/Base.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AccountSelectInputBase, {
  AccountSelectInputBaseProps,
} from "../AccountSelectInputBase/AccountSelectInputBase";

const BaseAccountSelectInput = ({
  ...props
}: AccountSelectInputBaseProps): React.ReactNode => {
  return (
    <AccountSelectInputBase
      classNames={{
        input: baseClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default BaseAccountSelectInput;
