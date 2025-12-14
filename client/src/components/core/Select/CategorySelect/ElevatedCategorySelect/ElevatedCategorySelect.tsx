import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import CategorySelectBase, {
  CategorySelectBaseProps,
} from "../CategorySelectBase/CategorySelectBase";

const ElevatedCategorySelect = ({
  ...props
}: CategorySelectBaseProps): React.ReactNode => {
  return (
    <CategorySelectBase
      classNames={{
        input: elevatedClasses.input,
      }}
      {...props}
    />
  );
};

export default ElevatedCategorySelect;
