import baseClasses from "~/styles/Base.module.css";

import React from "react";
import CategorySelectBase, {
  CategorySelectBaseProps,
} from "../CategorySelectBase/CategorySelectBase";

const BaseCategorySelect = ({
  ...props
}: CategorySelectBaseProps): React.ReactNode => {
  return (
    <CategorySelectBase
      classNames={{
        input: baseClasses.input,
      }}
      {...props}
    />
  );
};

export default BaseCategorySelect;
