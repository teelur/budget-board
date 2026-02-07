import baseClasses from "~/styles/Base.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AccountMultiSelectInputBase, {
  AccountMultiSelectInputBaseProps,
} from "../AccountMultiSelectInputBase/AccountMultiSelectInputBase";

const BaseAccountMultiSelectInput = ({
  ...props
}: AccountMultiSelectInputBaseProps): React.ReactNode => {
  return (
    <AccountMultiSelectInputBase
      classNames={{
        input: baseClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default BaseAccountMultiSelectInput;
