export interface IOidcCallbackRequest {
  code: string;
}

export interface IOidcCallbackResponse {
  success: boolean;
  error?: string;
}
