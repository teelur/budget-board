import elevatedClasses from "~/styles/Elevated.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AccountMultiSelectInputBase, {
  AccountMultiSelectInputBaseProps,
} from "../AccountMultiSelectInputBase/AccountMultiSelectInputBase";

const ElevatedAccountMultiSelectInput = ({
  ...props
}: AccountMultiSelectInputBaseProps): React.ReactNode => {
  return (
    <AccountMultiSelectInputBase
      classNames={{
        input: elevatedClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default ElevatedAccountMultiSelectInput;
