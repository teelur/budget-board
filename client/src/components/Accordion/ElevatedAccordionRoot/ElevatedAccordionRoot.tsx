import elevatedClasses from "~/styles/Elevated.module.css";

import React from "react";
import { Accordion, AccordionProps } from "@mantine/core";

export interface ElevatedAccordionRootProps extends AccordionProps<true> {
  children: React.ReactNode;
}

const ElevatedAccordionRoot = ({
  children,
  ...props
}: ElevatedAccordionRootProps): React.ReactNode => {
  return (
    <Accordion
      classNames={{
        item: elevatedClasses.accordion,
        control: elevatedClasses.accordion,
        panel: elevatedClasses.accordion,
      }}
      variant="separated"
      multiple
      {...props}
    >
      {children}
    </Accordion>
  );
};

export default ElevatedAccordionRoot;
