import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import CategorySelectBase, {
  CategorySelectBaseProps,
} from "../CategorySelectBase/CategorySelectBase";

export interface SurfaceCategorySelectProps extends CategorySelectBaseProps {}

const SurfaceCategorySelect = ({
  ...props
}: SurfaceCategorySelectProps): React.ReactNode => {
  return (
    <CategorySelectBase
      classNames={{
        input: surfaceClasses.input,
      }}
      {...props}
    />
  );
};

export default SurfaceCategorySelect;
