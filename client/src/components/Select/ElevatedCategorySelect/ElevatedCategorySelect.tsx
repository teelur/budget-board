import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import CategorySelectBase, {
  CategorySelectBaseProps,
} from "../CategorySelectBase/CategorySelectBase";

export interface ElevatedCategorySelectProps extends CategorySelectBaseProps {}

const ElevatedCategorySelect = ({
  ...props
}: ElevatedCategorySelectProps): React.ReactNode => {
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
