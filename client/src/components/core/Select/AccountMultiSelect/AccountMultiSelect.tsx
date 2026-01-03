import React from "react";
import { AccountMultiSelectInputBaseProps } from "./AccountMultiSelectInputBase/AccountMultiSelectInputBase";
import BaseAccountMultiSelectInput from "./BaseAccountMultiSelectInput/BaseAccountMultiSelectInput";
import SurfaceAccountMultiSelectInput from "./SurfaceAccountMultiSelectInput/SurfaceAccountMultiSelectInput";
import ElevatedAccountMultiSelectInput from "./ElevatedAccountMultiSelectInput/ElevatedAccountMultiSelectInput";

export interface AccountMultiSelectInputProps
  extends AccountMultiSelectInputBaseProps {
  elevation?: number;
}

const AccountMultiSelect = ({
  elevation = 0,
  ...props
}: AccountMultiSelectInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseAccountMultiSelectInput {...props} />;
    case 1:
      return <SurfaceAccountMultiSelectInput {...props} />;
    case 2:
      return <ElevatedAccountMultiSelectInput {...props} />;
    default:
      return null;
  }
};

export default AccountMultiSelect;
