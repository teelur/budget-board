import classes from "./AccordionItem.module.css";

import React from "react";
import { ChevronDown } from "lucide-react";

interface AccordionItemProps {
  title: React.ReactNode;
  defaultOpen?: boolean;
  slim?: boolean;
  children: React.ReactNode;
}

const AccordionItem = ({
  title,
  defaultOpen = true,
  slim = false,
  children,
}: AccordionItemProps): React.ReactNode => {
  const [isOpen, setIsOpen] = React.useState(defaultOpen);
  const panelId = React.useId();

  return (
    <div className={classes.item}>
      <button
        type="button"
        className={`${classes.control} ${slim ? classes.controlSlim : ""}`}
        aria-expanded={isOpen}
        aria-controls={panelId}
        onClick={() => setIsOpen((prev) => !prev)}
      >
        <span style={{ flex: 1 }}>{title}</span>
        <ChevronDown
          size={16}
          className={`${classes.chevron} ${isOpen ? classes.chevronOpen : ""}`}
        />
      </button>
      <div
        id={panelId}
        role="region"
        className={`${classes.panel} ${isOpen ? classes.panelOpen : ""}`}
        inert={!isOpen || undefined}
      >
        <div className={classes.panelInner}>
          <div
            className={`${classes.panelContent} ${isOpen ? (slim ? classes.panelContentOpenSlim : classes.panelContentOpen) : ""}`}
          >
            {children}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AccordionItem;
