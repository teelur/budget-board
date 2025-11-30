import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import AccountSelectInputBase, {
  AccountSelectInputBaseProps,
} from "../AccountSelectInputBase/AccountSelectInputBase";

export interface SurfaceAccountSelectInputProps
  extends AccountSelectInputBaseProps {}

const SurfaceAccountSelectInput = ({
  ...props
}: SurfaceAccountSelectInputProps): React.ReactNode => {
  return (
    <AccountSelectInputBase
      classNames={{
        input: surfaceClasses.input,
      }}
      {...props}
    />
  );
};

export default SurfaceAccountSelectInput;
