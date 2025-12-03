import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import CategorySelectBase, {
  CategorySelectBaseProps,
} from "../CategorySelectBase/CategorySelectBase";

const SurfaceCategorySelect = ({
  ...props
}: CategorySelectBaseProps): React.ReactNode => {
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
