import { Stack } from "@mantine/core";
import Budgets from "./Budgets/Budgets";
import Dashboard from "./Dashboard/Dashboard";
import Goals from "./Goals/Goals";
import Settings from "./Settings/Settings";
import Transactions from "./Transactions/Transactions";
import Trends from "./Trends/Trends";
import Accounts from "./Accounts/Accounts";
import Assets from "./Assets/Assets";
import ExternalAccounts from "./ExternalAccounts/ExternalAccounts";

export enum Pages {
  Dashboard,
  Accounts,
  Assets,
  Transactions,
  Budgets,
  Goals,
  Trends,
  ExternalAccounts,
  Settings,
}

interface PageContentProps {
  currentPage: Pages;
}

const PageContent = (props: PageContentProps): React.ReactNode => {
  const getPageContent = (page: Pages): React.ReactNode => {
    switch (page) {
      case Pages.Dashboard:
        return <Dashboard />;
      case Pages.Accounts:
        return <Accounts />;
      case Pages.Assets:
        return <Assets />;
      case Pages.Transactions:
        return <Transactions />;
      case Pages.Budgets:
        return <Budgets />;
      case Pages.Goals:
        return <Goals />;
      case Pages.Trends:
        return <Trends />;
      case Pages.ExternalAccounts:
        return <ExternalAccounts />;
      case Pages.Settings:
        return <Settings />;
    }
  };

  return (
    <Stack
      align="center"
      justify="flex-start"
      w="100%"
      h="100%"
      flex="1 1 auto"
    >
      {getPageContent(props.currentPage)}
    </Stack>
  );
};

export default PageContent;
