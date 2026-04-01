export interface IOidcCallbackRequest {
  code: string;
  redirect_uri: string;
}

export interface IOidcCallbackResponse {
  success: boolean;
  error?: string;
}

export interface IOidcDiscoveryDocument {
  issuer: string;
  authorization_endpoint: string;
  token_endpoint: string;
  userinfo_endpoint: string;
  jwks_uri: string;
  end_session_endpoint?: string;
  registration_endpoint?: string;
  scopes_supported: string[];
  response_types_supported: string[];
  subject_types_supported: string[];
  response_modes_supported: string[];
  grant_types_supported: string[];
  subject_type_supported: string;
  id_token_signing_alg_values_supported: string[];
  token_endpoint_auth_methods_supported: string[];
  claims_supported: string[];
}
