import React from "react";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";

const OidcCallback = (): React.ReactNode => {
  const { request, setIsUserAuthenticated } =
    React.useContext<any>(AuthContext);

  console.log("OIDC Callback loaded");

  React.useEffect(() => {
    (async () => {
      console.log("Processing OIDC callback");

      const q = new URLSearchParams(window.location.search);
      const code = q.get("code");
      const state = q.get("state");
      const saved = sessionStorage.getItem("oidc_state");
      sessionStorage.removeItem("oidc_state");

      if (!code) {
        notifications.show({
          color: "red",
          message: "OIDC callback missing code.",
        });
        window.location.href = "/";
        return;
      }

      if (!state || state !== saved) {
        notifications.show({ color: "red", message: "Invalid OIDC state." });
        window.location.href = "/";
        return;
      }

      try {
        // POST to your backend to exchange the code for a session (implement on server)
        await request({
          url: "/api/oidc/callback",
          method: "POST",
          data: {
            code,
            redirectUri: `${window.location.origin}/oidc-callback`,
          },
        });
        setIsUserAuthenticated(true);
      } catch (e) {
        const err = e as AxiosError;
        notifications.show({ color: "red", message: translateAxiosError(err) });
      } finally {
        // redirect to app home
        window.location.href = "/";
      }
    })();
  }, []);

  return <div>Signing you in…</div>;
};

export default OidcCallback;
