import classes from "./AccordionItem.module.css";

import React from "react";
import { ChevronDown } from "lucide-react";

interface AccordionItemProps {
  title: React.ReactNode;
  defaultOpen?: boolean;
  children: React.ReactNode;
}

const AccordionItem = ({
  title,
  defaultOpen = true,
  children,
}: AccordionItemProps): React.ReactNode => {
  const [isOpen, setIsOpen] = React.useState(defaultOpen);

  return (
    <div className={classes.item}>
      <button
        type="button"
        className={classes.control}
        onClick={() => setIsOpen((prev) => !prev)}
      >
        <span style={{ flex: 1 }}>{title}</span>
        <ChevronDown
          size={16}
          className={`${classes.chevron} ${isOpen ? classes.chevronOpen : ""}`}
        />
      </button>
      <div className={`${classes.panel} ${isOpen ? classes.panelOpen : ""}`}>
        <div className={classes.panelInner}>
          <div
            className={`${classes.panelContent} ${isOpen ? classes.panelContentOpen : ""}`}
          >
            {children}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AccordionItem;
