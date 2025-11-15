/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_CLIENT_URL: string;
  readonly VITE_OIDC_ENABLED: string;
  readonly VITE_OIDC_PROVIDER: string;
  readonly VITE_OIDC_CLIENT_ID: string;
  readonly VITE_DISABLE_LOCAL_AUTH: string;
  readonly VITE_DISABLE_NEW_USERS: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
