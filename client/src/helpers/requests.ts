import { AxiosError } from "axios";

export interface ValidationError {
  title: string;
  type: string;
  status: number;
  errors: object;
}

/**
 * Translates an Axios error object into a human-readable error message.
 *
 * @param error - The AxiosError object to translate.
 * @returns A string describing the error, based on the error's response, request, or setup.
 *
 * - If the server responded with an error message (as a string), that message is returned.
 * - If the request was made but no response was received, a generic message is returned.
 * - If the error occurred during request setup, a setup error message is returned.
 */
export const translateAxiosError = (error: AxiosError): string => {
  if (error.response?.data) {
    if (typeof error.response.data === "string") {
      // The request was made and the server responded with a status code
      // that falls out of the range of 2xx
      return error.response.data;
    } else if (typeof error.response.data === "object" && error.response.data.message) {
      // The server responded with a JSON-style error containing a `message` property
      return error.response.data.message;
    }
  } else if (error.request) {
    // The request was made but no response was received
    return "No response received from the server.";
  }
  // Something happened in setting up the request that triggered an Error
  return "An error occurred while setting up the request.";
};
