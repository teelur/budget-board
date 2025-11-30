import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import CategorySelect, {
  CategorySelectProps,
} from "../CategorySelect/CategorySelect";
interface ElevatedCategorySelectProps extends CategorySelectProps {}

const ElevatedCategorySelect = ({
  ...props
}: ElevatedCategorySelectProps): React.ReactNode => {
  return (
    <CategorySelect
      classNames={{
        input: elevatedClasses.input,
      }}
      {...props}
    />
  );
};

export default ElevatedCategorySelect;
