import BaseCategorySelect from "./BaseCategorySelect/BaseCategorySelect";
import { CategorySelectBaseProps } from "./CategorySelectBase/CategorySelectBase";
import ElevatedCategorySelect from "./ElevatedCategorySelect/ElevatedCategorySelect";
import SurfaceCategorySelect from "./SurfaceCategorySelect/SurfaceCategorySelect";

export interface CategorySelectProps extends CategorySelectBaseProps {
  elevation?: number;
}

const CategorySelect = ({
  elevation = 0,
  ...props
}: CategorySelectProps): React.ReactNode => {
  switch (elevation) {
    case 0:
      return <BaseCategorySelect {...props} />;
    case 1:
      return <SurfaceCategorySelect {...props} />;
    case 2:
      return <ElevatedCategorySelect {...props} />;
    default:
      return null;
  }
};

export default CategorySelect;
