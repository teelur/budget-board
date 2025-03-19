import Budgets from "./Budgets/Budgets";
import Dashboard from "./Dashboard/Dashboard";
import Goals from "./Goals/Goals";
import Settings from "./Settings/Settings";
import Transactions from "./Transactions/Transactions";
import Trends from "./Trends/Trends";

export enum Pages {
  Dashboard,
  Transactions,
  Budgets,
  Goals,
  Trends,
  Settings,
}

interface PageContentProps {
  currentPage: Pages;
}

const PageContent = (props: PageContentProps): React.ReactNode => {
  switch (props.currentPage) {
    case Pages.Dashboard:
      return <Dashboard />;
    case Pages.Transactions:
      return <Transactions />;
    case Pages.Budgets:
      return <Budgets />;
    case Pages.Goals:
      return <Goals />;
    case Pages.Trends:
      return <Trends />;
    case Pages.Settings:
      return <Settings />;
  }
};

export default PageContent;
