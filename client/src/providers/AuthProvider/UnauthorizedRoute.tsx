import { useContext } from "react";
import { AuthContext } from "./AuthProvider";
import { Navigate } from "react-router";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";
import { useTranslation } from "react-i18next";

interface UnauthorizedRouteProps {
  children: React.ReactNode;
}

const UnauthorizedRoute = (props: UnauthorizedRouteProps): React.ReactNode => {
  const { isUserAuthenticated, loading } = useContext(AuthContext);
  const { t } = useTranslation();

  if (loading) {
    return <LoadingScreen message={t("checking_session")} />;
  }

  if (!isUserAuthenticated) {
    return props.children;
  }

  return <Navigate to="/dashboard" />;
};

export default UnauthorizedRoute;
