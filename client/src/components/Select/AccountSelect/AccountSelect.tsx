import React from "react";
import SurfaceAccountSelectInput, {
  SurfaceAccountSelectInputProps,
} from "./SurfaceAccountSelectInput/SurfaceAccountSelectInput";

export interface AccountSelectInputProps
  extends SurfaceAccountSelectInputProps {
  elevation?: number;
}

const AccountSelect = ({
  elevation = 0,
  ...props
}: AccountSelectInputProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      throw new Error("Base is not supported for AccountSelectInput");
    case 1:
      return <SurfaceAccountSelectInput {...props} />;
    case 2:
      throw new Error("Elevated is not supported for AccountSelectInput");
    default:
      return <SurfaceAccountSelectInput {...props} />;
  }
};

export default AccountSelect;
