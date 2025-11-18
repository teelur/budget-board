import React from "react";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import {
  AuthContext,
  AuthContextValue,
} from "~/providers/AuthProvider/AuthProvider";
import { useNavigate } from "react-router";
import { Center, Loader } from "@mantine/core";
import { IOidcCallbackRequest, IOidcCallbackResponse } from "~/models/oidc";

const OidcCallback = (): React.ReactNode => {
  const { request, setIsUserAuthenticated } =
    React.useContext<AuthContextValue>(AuthContext);
  const navigate = useNavigate();

  const hasProcessed = React.useRef(false);

  React.useEffect(() => {
    if (hasProcessed.current) {
      return;
    }

    hasProcessed.current = true;

    (async () => {
      const q = new URLSearchParams(window.location.search);
      const code = q.get("code");
      const state = q.get("state");
      const error = q.get("error");
      const errorDescription = q.get("error_description");
      const saved = sessionStorage.getItem("oidc_state");

      // Clear state immediately to prevent reuse
      sessionStorage.removeItem("oidc_state");

      // Check for OAuth2 error response
      if (error) {
        notifications.show({
          color: "red",
          message: `Authentication failed: ${errorDescription || error}`,
        });
        navigate("/");
        return;
      }

      if (!code) {
        notifications.show({
          color: "red",
          message: "OIDC callback missing code.",
        });
        navigate("/");
        return;
      }

      if (!state || state !== saved) {
        notifications.show({ color: "red", message: "Invalid OIDC state." });
        navigate("/");
        return;
      }

      try {
        const response: AxiosResponse<IOidcCallbackResponse> = await request({
          url: "/api/oidc/callback",
          method: "POST",
          data: {
            code,
            redirect_uri: `${window.location.origin}/oidc-callback`,
          } as IOidcCallbackRequest,
        });

        setIsUserAuthenticated(response.data?.success ?? false);

        if (!response.data?.success) {
          notifications.show({
            color: "red",
            message: "Authentication failed.",
          });
          navigate("/");
          return;
        }

        navigate("/dashboard");
      } catch (e) {
        const err = e as AxiosError;
        notifications.show({ color: "red", message: translateAxiosError(err) });
        navigate("/");
      }
    })();
  }, []);

  return (
    <Center h="100vh">
      <Loader size={100} />
    </Center>
  );
};

export default OidcCallback;
