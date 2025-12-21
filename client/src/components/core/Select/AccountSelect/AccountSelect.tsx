import React from "react";
import SurfaceAccountSelectInput from "./SurfaceAccountSelectInput/SurfaceAccountSelectInput";
import BaseAccountSelectInput from "./BaseAccountSelectInput/BaseAccountSelectInput";
import { AccountSelectInputBaseProps } from "./AccountSelectInputBase/AccountSelectInputBase";
import ElevatedAccountSelectInput from "./ElevatedAccountSelectInput/ElevatedAccountSelectInput";

export interface AccountSelectInputProps extends AccountSelectInputBaseProps {
  elevation?: number;
}

const AccountSelect = ({
  elevation = 0,
  ...props
}: AccountSelectInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseAccountSelectInput {...props} />;
    case 1:
      return <SurfaceAccountSelectInput {...props} />;
    case 2:
      return <ElevatedAccountSelectInput {...props} />;
    default:
      return null;
  }
};

export default AccountSelect;
