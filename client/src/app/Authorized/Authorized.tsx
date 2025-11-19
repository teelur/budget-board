import {
  AppShell,
  AppShellHeader,
  AppShellMain,
  AppShellNavbar,
} from "@mantine/core";
import Navbar from "./Navbar/Navbar";
import React from "react";
import PageContent, { Pages } from "./PageContent/PageContent";
import Header from "./Header/Header";
import { useDisclosure } from "@mantine/hooks";
import { TransactionFiltersProvider } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { TransactionCategoryProvider } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

const Authorized = (): React.ReactNode => {
  const [currentPage, setCurrentPage] = React.useState(Pages.Dashboard);
  const [isNavbarOpen, { toggle }] = useDisclosure();

  const onPageSelect = (page: Pages): void => {
    setCurrentPage(page);
    toggle();
  };

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
      padding={12}
    >
      <AppShellHeader bg="var(--mantine-color-header-background)">
        <Header isNavbarOpen={isNavbarOpen} toggleNavbar={toggle} />
      </AppShellHeader>
      <AppShellNavbar bg="var(--mantine-color-sidebar-background)">
        <Navbar
          currentPage={currentPage}
          setCurrentPage={onPageSelect}
          isNavbarOpen={isNavbarOpen}
          toggleNavbar={toggle}
        />
      </AppShellNavbar>
      <AppShellMain
        bg="var(--mantine-color-content-background)"
        flex={{ direction: "column" }}
      >
        <TransactionCategoryProvider>
          <TransactionFiltersProvider setCurrentPage={setCurrentPage}>
            <PageContent currentPage={currentPage} />
          </TransactionFiltersProvider>
        </TransactionCategoryProvider>
      </AppShellMain>
    </AppShell>
  );
};

export default Authorized;
