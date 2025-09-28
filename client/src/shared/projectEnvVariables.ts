/* eslint-disable no-template-curly-in-string */
type ProjectEnvVariablesType = Pick<
  ImportMetaEnv,
  | "VITE_API_URL"
  | "VITE_OIDC_PROVIDER"
  | "VITE_OIDC_CLIENT_ID"
  | "VITE_OIDC_CLIENT_SECRET"
>;

// Environment Variable Template to Be Replaced at Runtime
const projectEnvVariables: ProjectEnvVariablesType = {
  VITE_API_URL: "${VITE_API_URL}",
  VITE_OIDC_PROVIDER: "${VITE_OIDC_PROVIDER}",
  VITE_OIDC_CLIENT_ID: "${VITE_OIDC_CLIENT_ID}",
  VITE_OIDC_CLIENT_SECRET: "${VITE_OIDC_CLIENT_SECRET}",
};

// Returning the variable value from runtime or obtained as a result of the build
export const getProjectEnvVariables = (): {
  envVariables: ProjectEnvVariablesType;
} => {
  return {
    envVariables: {
      VITE_API_URL: !projectEnvVariables.VITE_API_URL.includes("VITE_")
        ? projectEnvVariables.VITE_API_URL
        : import.meta.env.VITE_API_URL,
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
      VITE_OIDC_CLIENT_SECRET:
        !projectEnvVariables.VITE_OIDC_CLIENT_SECRET?.includes("VITE_")
          ? projectEnvVariables.VITE_OIDC_CLIENT_SECRET
          : import.meta.env.VITE_OIDC_CLIENT_SECRET,
    },
  };
};
