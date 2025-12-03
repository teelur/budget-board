import { useContext } from "react";
import { AuthContext } from "./AuthProvider";
import { Navigate } from "react-router";
import { Center, Loader } from "@mantine/core";

interface UnauthorizedRouteProps {
  children: React.ReactNode;
}

const UnauthorizedRoute = (props: UnauthorizedRouteProps): React.ReactNode => {
  const { isUserAuthenticated, loading } = useContext(AuthContext);

  if (loading) {
    return (
      <Center h="100vh">
        <Loader size={100} />
      </Center>
    );
  }

  if (!isUserAuthenticated) {
    return props.children;
  }

  return <Navigate to="/dashboard" />;
};

export default UnauthorizedRoute;
