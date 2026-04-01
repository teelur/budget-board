import baseClasses from "~/styles/Base.module.css";

import React from "react";
import { Accordion, AccordionProps } from "@mantine/core";

export interface BaseAccordionRootProps extends AccordionProps<true> {
  children: React.ReactNode;
}

const BaseAccordionRoot = ({
  children,
  ...props
}: BaseAccordionRootProps): React.ReactNode => {
  return (
    <Accordion
      classNames={{
        item: baseClasses.accordion,
        control: baseClasses.accordion,
        panel: baseClasses.accordion,
      }}
      variant="separated"
      multiple
      {...props}
    >
      {children}
    </Accordion>
  );
};

export default BaseAccordionRoot;
