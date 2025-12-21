import BaseAccordionRoot, {
  BaseAccordionRootProps,
} from "./BaseAccordionRoot/BaseAccordionRoot";
import ElevatedAccordionRoot, {
  ElevatedAccordionRootProps,
} from "./ElevatedAccordionRoot/ElevatedAccordionRoot";
import SurfaceAccordionRoot, {
  SurfaceAccordionRootProps,
} from "./SurfaceAccordionRoot/SurfaceAccordionRoot";

interface AccordionProps
  extends BaseAccordionRootProps,
    SurfaceAccordionRootProps,
    ElevatedAccordionRootProps {
  elevation?: number;
}

const Accordion = ({ elevation, children, ...props }: AccordionProps) => {
  switch (elevation) {
    case 0:
      return <BaseAccordionRoot {...props}>{children}</BaseAccordionRoot>;
    case 1:
      return <SurfaceAccordionRoot {...props}>{children}</SurfaceAccordionRoot>;
    case 2:
      return (
        <ElevatedAccordionRoot {...props}>{children}</ElevatedAccordionRoot>
      );
    default:
      return null;
  }
};

export default Accordion;
