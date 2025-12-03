import baseClasses from "~/styles/Base.module.css";

import React, { ReactNode } from "react";
import {
  DrawerProps as MantineDrawerProps,
  Drawer as MantineDrawer,
} from "@mantine/core";

interface DrawerProps extends MantineDrawerProps {
  children: ReactNode;
}

const Drawer = ({ children, ...props }: DrawerProps): React.ReactNode => {
  return (
    <MantineDrawer
      classNames={{
        header: baseClasses.root,
        content: baseClasses.root,
      }}
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
      {...props}
    >
      {children}
    </MantineDrawer>
  );
};

export default Drawer;
