import surfaceClasses from "~/styles/Surface.module.css";

import React from "react";
import { Accordion, AccordionProps } from "@mantine/core";

export interface SurfaceAccordionRootProps extends AccordionProps<true> {
  children: React.ReactNode;
}

const SurfaceAccordionRoot = ({
  children,
  ...props
}: SurfaceAccordionRootProps): React.ReactNode => {
  return (
    <Accordion
      classNames={{
        item: surfaceClasses.accordion,
        control: surfaceClasses.accordion,
        panel: surfaceClasses.accordion,
      }}
      variant="separated"
      multiple
      {...props}
    >
      {children}
    </Accordion>
  );
};

export default SurfaceAccordionRoot;
