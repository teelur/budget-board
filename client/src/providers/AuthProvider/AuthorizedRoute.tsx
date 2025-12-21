import { useContext } from "react";
import { AuthContext } from "./AuthProvider";
import { Navigate } from "react-router";
import LoadingScreen from "~/components/LoadingScreen/LoadingScreen";

interface AuthRouteProps {
  children: React.ReactNode;
}

const AuthorizedRoute = (props: AuthRouteProps): React.ReactNode => {
  const { isUserAuthenticated, loading } = useContext(AuthContext);

  if (loading) {
    return <LoadingScreen />;
  }

  if (isUserAuthenticated) {
    return props.children;
  }

  return <Navigate to="/" />;
};

export default AuthorizedRoute;
