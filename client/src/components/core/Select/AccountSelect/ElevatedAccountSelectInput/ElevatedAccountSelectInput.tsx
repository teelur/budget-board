import elevatedClasses from "~/styles/Elevated.module.css";
import dropdownClasses from "~/styles/Dropdown.module.css";

import React from "react";
import AccountSelectInputBase, {
  AccountSelectInputBaseProps,
} from "../AccountSelectInputBase/AccountSelectInputBase";

const ElevatedAccountSelectInput = ({
  ...props
}: AccountSelectInputBaseProps): React.ReactNode => {
  return (
    <AccountSelectInputBase
      classNames={{
        input: elevatedClasses.input,
        dropdown: dropdownClasses.dropdown,
      }}
      {...props}
    />
  );
};

export default ElevatedAccountSelectInput;
