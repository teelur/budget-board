import React from "react";
import { Stack, StackProps } from "@mantine/core";
import AccordionItem from "./AccordionItem/AccordionItem";

const BG_VARS: Record<number, string> = {
  0: "var(--background-color-base)",
  1: "var(--background-color-surface)",
  2: "var(--background-color-elevated)",
};

const BORDER_VARS: Record<number, string> = {
  0: "var(--base-color-border)",
  1: "var(--surface-color-border)",
  2: "var(--elevated-color-border)",
};

interface AccordionProps extends Omit<StackProps, "children"> {
  elevation?: 0 | 1 | 2;
  children: React.ReactNode;
}

const Accordion: ((props: AccordionProps) => React.ReactNode) & {
  Item: typeof AccordionItem;
} = ({
  elevation = 0,
  children,
  style,
  ...props
}: AccordionProps): React.ReactNode => {
  return (
    <Stack
      gap={props.gap ?? "0.5rem"}
      {...props}
      style={{
        "--accordion-bg": BG_VARS[elevation],
        "--accordion-border": BORDER_VARS[elevation],
        ...style,
      }}
    >
      {children}
    </Stack>
  );
};

Accordion.Item = AccordionItem;

export default Accordion;
