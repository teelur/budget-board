import { useContext } from 'react';
import { AuthContext } from './auth-provider';
import PropTypes from 'prop-types';
import { Navigate } from 'react-router-dom';

const AuthRoute = ({ children }: { children: any }): JSX.Element => {
  const { currentUserState, loading } = useContext<any>(AuthContext);

  // eslint-disable-next-line @typescript-eslint/strict-boolean-expressions
  if (loading) {
    // TODO: Create a better loading screen
    return <p>Loading...</p>;
  }
  if (currentUserState !== null) {
    // eslint-disable-next-line no-extra-boolean-cast
    console.log(currentUserState.emailVerified);
    if (currentUserState.emailVerified) {
      return children;
    }
  }
  return <Navigate to="/" />;
};

AuthRoute.propTypes = {
  children: PropTypes.node,
};

export default AuthRoute;
