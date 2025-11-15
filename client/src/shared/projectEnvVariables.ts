/* eslint-disable no-template-curly-in-string */
type ProjectEnvVariablesType = Pick<
  ImportMetaEnv,
  | "VITE_CLIENT_URL"
  | "VITE_OIDC_ENABLED"
  | "VITE_OIDC_PROVIDER"
  | "VITE_OIDC_CLIENT_ID"
  | "VITE_DISABLE_LOCAL_AUTH"
  | "VITE_DISABLE_NEW_USERS"
>;

// Environment Variable Template to Be Replaced at Runtime
const projectEnvVariables: ProjectEnvVariablesType = {
  VITE_CLIENT_URL: "${VITE_CLIENT_URL}",
  VITE_OIDC_ENABLED: "${VITE_OIDC_ENABLED}",
  VITE_OIDC_PROVIDER: "${VITE_OIDC_PROVIDER}",
  VITE_OIDC_CLIENT_ID: "${VITE_OIDC_CLIENT_ID}",
  VITE_DISABLE_LOCAL_AUTH: "${VITE_DISABLE_LOCAL_AUTH}",
  VITE_DISABLE_NEW_USERS: "${VITE_DISABLE_NEW_USERS}",
};

// Returning the variable value from runtime or obtained as a result of the build
export const getProjectEnvVariables = (): {
  envVariables: ProjectEnvVariablesType;
} => {
  return {
    envVariables: {
      VITE_CLIENT_URL: !projectEnvVariables.VITE_CLIENT_URL.includes("VITE_")
        ? projectEnvVariables.VITE_CLIENT_URL
        : import.meta.env.VITE_CLIENT_URL,
      VITE_OIDC_ENABLED: !projectEnvVariables.VITE_OIDC_ENABLED?.includes(
        "VITE_"
      )
        ? projectEnvVariables.VITE_OIDC_ENABLED
        : import.meta.env.VITE_OIDC_ENABLED,
      VITE_OIDC_PROVIDER: !projectEnvVariables.VITE_OIDC_PROVIDER?.includes(
        "VITE_"
      )
        ? projectEnvVariables.VITE_OIDC_PROVIDER
        : import.meta.env.VITE_OIDC_PROVIDER,
      VITE_OIDC_CLIENT_ID: !projectEnvVariables.VITE_OIDC_CLIENT_ID?.includes(
        "VITE_"
      )
        ? projectEnvVariables.VITE_OIDC_CLIENT_ID
        : import.meta.env.VITE_OIDC_CLIENT_ID,
      VITE_DISABLE_LOCAL_AUTH:
        !projectEnvVariables.VITE_DISABLE_LOCAL_AUTH?.includes("VITE_")
          ? projectEnvVariables.VITE_DISABLE_LOCAL_AUTH
          : import.meta.env.VITE_DISABLE_LOCAL_AUTH,
      VITE_DISABLE_NEW_USERS:
        !projectEnvVariables.VITE_DISABLE_NEW_USERS?.includes("VITE_")
          ? projectEnvVariables.VITE_DISABLE_NEW_USERS
          : import.meta.env.VITE_DISABLE_NEW_USERS,
    },
  };
};
