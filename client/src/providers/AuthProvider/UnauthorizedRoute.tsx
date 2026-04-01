import { useContext } from "react";
import { AuthContext } from "./AuthProvider";
import { Navigate } from "react-router";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";

interface UnauthorizedRouteProps {
  children: React.ReactNode;
}

const UnauthorizedRoute = (props: UnauthorizedRouteProps): React.ReactNode => {
  const { isUserAuthenticated, loading } = useContext(AuthContext);

  if (loading) {
    return <LoadingScreen />;
  }

  if (!isUserAuthenticated) {
    return props.children;
  }

  return <Navigate to="/dashboard" />;
};

export default UnauthorizedRoute;
