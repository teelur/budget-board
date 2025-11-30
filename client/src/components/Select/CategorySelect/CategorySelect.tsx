import ElevatedCategorySelect, {
  ElevatedCategorySelectProps,
} from "../ElevatedCategorySelect/ElevatedCategorySelect";
import SurfaceCategorySelect, {
  SurfaceCategorySelectProps,
} from "../SurfaceCategorySelect/SurfaceCategorySelect";

export interface CategorySelectProps
  extends SurfaceCategorySelectProps,
    ElevatedCategorySelectProps {
  elevation?: number;
}

const CategorySelect = ({
  elevation,
  ...props
}: CategorySelectProps): React.ReactNode => {
  switch (elevation) {
    case 1:
      return <SurfaceCategorySelect {...props} />;
    case 2:
      return <ElevatedCategorySelect {...props} />;
    default:
      return <SurfaceCategorySelect {...props} />;
  }
};

export default CategorySelect;
