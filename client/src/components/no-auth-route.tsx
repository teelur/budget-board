/* eslint-disable @typescript-eslint/strict-boolean-expressions */
import { useContext } from 'react';
import { AuthContext } from './auth-provider';
import PropTypes from 'prop-types';
import { Navigate } from 'react-router-dom';
import PageLoading from './page-loading';

const NoAuthRoute = ({ children }: { children: any }): JSX.Element => {
  const { currentUserState, loading } = useContext<any>(AuthContext);

  if (loading) {
    return <PageLoading />;
  }
  if (currentUserState == null) {
    return children;
  }

  return <Navigate to="/dashboard" />;
};

NoAuthRoute.propTypes = {
  children: PropTypes.node,
};

export default NoAuthRoute;
