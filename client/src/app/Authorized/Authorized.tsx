import React from "react";
import {
  AppShell,
  AppShellHeader,
  AppShellMain,
  AppShellNavbar,
} from "@teelur/budget-board-ui";
import Navbar from "./Navbar/Navbar";
import PageContent from "./PageContent/PageContent";
import Header from "./Header/Header";
import { useDisclosure, useMediaQuery } from "@mantine/hooks";
import { TransactionFiltersProvider } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { TransactionCategoryProvider } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { AccountTypeProvider } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { AssetTypeProvider } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { TransactionImportJobProvider } from "~/providers/TransactionImportJobProvider/TransactionImportJobProvider";
import TransactionImportJobPanel from "~/components/TransactionImportJobPanel/TransactionImportJobPanel";

const Authorized = (): React.ReactNode => {
  const [isNavbarOpen, { toggle, close }] = useDisclosure();
  const isMobile = useMediaQuery(
    "(max-width: 29.999em)",
    typeof window !== "undefined" &&
      window.matchMedia("(max-width: 29.999em)").matches,
    {
      getInitialValueInEffect: false,
    },
  );
  const isLargeScreen = useMediaQuery(
    "(min-width: 75em)",
    typeof window !== "undefined" &&
      window.matchMedia("(min-width: 75em)").matches,
    {
      getInitialValueInEffect: false,
    },
  );
  const [isNavbarExpanded, setIsNavbarExpanded] = React.useState(isLargeScreen);

  React.useEffect(() => {
    setIsNavbarExpanded(isLargeScreen);
  }, [isLargeScreen]);

  return (
    <AppShell
      layout="alt"
      withBorder
      transitionDuration={0}
      navbar={{
        width: isMobile ? "100vw" : isNavbarExpanded ? 220 : 60,
        breakpoint: "xs",
        collapsed: { mobile: !isNavbarOpen },
      }}
      header={{
        height: 60,
      }}
      p={0}
    >
      <AppShellHeader>
        <Header isNavbarOpen={isNavbarOpen} toggleNavbar={toggle} />
      </AppShellHeader>
      <AppShellNavbar>
        <Navbar
          isNavbarOpen={isNavbarOpen}
          toggleNavbar={toggle}
          closeNavbar={close}
          isMobile={isMobile}
          isNavbarExpanded={isNavbarExpanded}
          toggleNavbarExpanded={() =>
            setIsNavbarExpanded((expanded) => !expanded)
          }
        />
      </AppShellNavbar>
      <AppShellMain h="100dvh" flex={{ direction: "column" }}>
        <AccountTypeProvider>
          <AssetTypeProvider>
            <TransactionCategoryProvider>
              <TransactionFiltersProvider>
                <TransactionImportJobProvider>
                  <PageContent />
                  <TransactionImportJobPanel />
                </TransactionImportJobProvider>
              </TransactionFiltersProvider>
            </TransactionCategoryProvider>
          </AssetTypeProvider>
        </AccountTypeProvider>
      </AppShellMain>
    </AppShell>
  );
};

export default Authorized;
