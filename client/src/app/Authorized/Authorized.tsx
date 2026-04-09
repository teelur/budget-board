import {
  AppShell,
  AppShellHeader,
  AppShellMain,
  AppShellNavbar,
} from "@mantine/core";
import Navbar from "./Navbar/Navbar";
import React from "react";
import PageContent from "./PageContent/PageContent";
import Header from "./Header/Header";
import { useDisclosure } from "@mantine/hooks";
import { TransactionFiltersProvider } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { TransactionCategoryProvider } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

const Authorized = (): React.ReactNode => {
  const [isNavbarOpen, { toggle }] = useDisclosure();

  return (
    <AppShell
      layout="alt"
      withBorder
      navbar={{
        width: 60,
        breakpoint: "xs",
        collapsed: { mobile: !isNavbarOpen },
      }}
      header={{
        height: 60,
      }}
      bg="var(--background-color-base)"
      p={0}
    >
      <AppShellHeader
        bg="var(--background-color-header)"
        style={{ borderWidth: "2px" }}
      >
        <Header isNavbarOpen={isNavbarOpen} toggleNavbar={toggle} />
      </AppShellHeader>
      <AppShellNavbar
        bg="var(--background-color-sidebar)"
        style={{ borderWidth: "2px" }}
      >
        <Navbar isNavbarOpen={isNavbarOpen} toggleNavbar={toggle} />
      </AppShellNavbar>
      <AppShellMain
        bg="var(--background-color-base)"
        h="100dvh"
        flex={{ direction: "column" }}
      >
        <TransactionCategoryProvider>
          <TransactionFiltersProvider>
            <PageContent />
          </TransactionFiltersProvider>
        </TransactionCategoryProvider>
      </AppShellMain>
    </AppShell>
  );
};

export default Authorized;
