import { AxiosError, type AxiosRequestConfig } from 'axios';
import { axiosApi } from './axios';
import { ref } from 'vue';
import { combinePathWithBaseUrl } from './url';
import { useAppStore } from '@/stores/app-store';

const showFailedMessage = (message: string): void => {
  console.error(message);
};

const serverValidationErrors = ref<ErrorModel[]>([]);

export const defaultConfig: AxiosRequestConfig = {
  timeout: 50000, // 50 seconds
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json'
  }
};

export enum ApiErrorType {
  Undefined = 0, // The error type is not defined (unknown)
  ConnectionFailed = 1, // Failed to connect to the server
  Timeout = 2, // Connected to server but time out awaiting response
  BadRequest = 400,
  Unauthorized = 401,
  PaymentRequired = 402,
  Forbidden = 403,
  NotFound = 404,
  NotAllowed = 405,
  NotAcceptable = 406,
  RequestTimeout = 408,
  Conflict = 409,
  Gone = 410,
  InternalServerError = 500,
  NotImplemented = 501,
  BadGateway = 502,
  ServiceUnavailable = 503,
  GatewayTimeout = 504
}

/**
 * An error from the API is expected to contain a list of ErrorModel objects
 * [
 *     {
 *         "property": null,
 *         "errorMessage": "Contact does not exist."
 *     }
 * ]
 */
export interface ApiError {
  errorType: ApiErrorType;
  errors: ErrorModel[];
}

/**
 * Maps to the ErrorModel class in the API
 */
export interface ErrorModel {
  property: string | null;
  errorMessage: string;
}

// Api caller can pass in this function to have first chance at handling error before the
// default error handler displays a message to the user
export interface HandleErrorCallback {
  (apiError: ApiError): boolean;
}

const handleApiResponseError = (err: unknown, url: string): ApiError => {
  // Convert to axios error type
  const error = err as AxiosError;

  // Create default error
  const apiError: ApiError = {
    errorType: ApiErrorType.Undefined,
    errors: [{ property: null, errorMessage: 'An undefined error occurred' }]
  };

  // If there is no response then likely to be a network error
  if (!error.response) {
    // This is a network error, eg:
    // net::ERR_CONNECTION_RESET
    // net::ERR_CONNECTION_REFUSED
    // net::ERR_NETWORK
    apiError.errorType = ApiErrorType.ConnectionFailed;
    apiError.errors = [{ property: null, errorMessage: `Failed to connect to the server at URL '${combinePathWithBaseUrl(url)}'.` }];
  }

  // Only call this bit if API errors not already set
  if (apiError && apiError.errors.length == 0 && error.code) {
    switch (error.code) {
      case 'ECONNABORTED': {
        // This is a timeout condition (ie connected to server OK, but the server did not respond within the timeout expiry)
        apiError.errorType = ApiErrorType.Timeout;
        apiError.errors = [
          {
            property: null,
            errorMessage: `The server failed to respond within ${(defaultConfig.timeout ?? 5000) / 1000} seconds.`
          }
        ];
        break;
      }

      default: {
        apiError.errors = [{ property: null, errorMessage: `Unhandled error code '${error.code}'.` }];
        break;
      }
    }
  }

  // There was an error response
  if (error.response) {
    // The status code is mapped to an error type
    apiError.errorType = error.response.status;

    if (error.response?.data) {
      // The message is mapped to the returned data
      const errors = error.response.data as ErrorModel[];
      apiError.errors = errors;
    }
  }

  // Return error
  return apiError;
};

export const handleApiError = (
  error: unknown,
  url: string,
  method: string,
  errorHandlerCallback?: HandleErrorCallback,
  suppressUnauthorizedError?: boolean
): ApiError => {
  const apiError = handleApiResponseError(error, url);

  // If validation errors have been returned, give them to the validation form
  if (apiError.errorType === ApiErrorType.BadRequest) {
    serverValidationErrors.value = apiError.errors;
  }

  let errorHandled = apiError.errorType === ApiErrorType.Unauthorized && suppressUnauthorizedError;

  // Did the caller pass in a custom error handler?
  if (!errorHandled && !!errorHandlerCallback) {
    // Check if the caller handled the error
    errorHandled = errorHandlerCallback(apiError);
  }

  // If the caller did not handle the error then display a generic error
  if (!errorHandled) {
    displayErrorMessage(apiError, method);
  }

  // Return error for further processing as needed
  return apiError;
};

const displayErrorMessage = (error: ApiError, method: string): void => {
  // A HTTP 409 (conflict) indicates update concurrency exception
  if (error.errorType === ApiErrorType.Conflict) {
    showFailedMessage(
      `${method} failed because the data has been modified by '${error.errors[0].errorMessage}' since you started editing.` +
        'Please reload the page and try again. Reloading will reset any changes that you have made.'
    );
  }

  // A HTTP 404 (not found)
  else if (error.errorType === ApiErrorType.NotFound) {
    showFailedMessage(`${method} failed because the item no longer exists.`);
  } else if (error.errorType === ApiErrorType.BadRequest) {
    showFailedMessage(`${method} failed with error '${error.errors[0].errorMessage}'.`);
  } else {
    showFailedMessage(`${method} failed. The server may be offline. Please try again later. [Error type: ${error.errorType}]`);
  }

  // Rethrow message in case called needs info
  throw error;
};

export const httpGet = async <T>(url: string, errorHandlerCallback?: HandleErrorCallback, retrying?: boolean): Promise<T> => {
  const config = {
    ...defaultConfig
  };

  // Flag we are waiting
  const appStore = useAppStore();
  appStore.incrementBusy();

  try {
    const response = await axiosApi.get(url, config);
    return response.data;
  } catch (err: unknown) {
    const apiError = handleApiError(err, url, 'GET', errorHandlerCallback, !retrying);

    throw apiError;
  } finally {
    appStore.decrementBusy();
  }
};

export const httpPost = async <TRequest, TResponse>(
  requestData: TRequest,
  url: string,
  errorHandlerCallback?: HandleErrorCallback,
  retrying?: boolean
): Promise<TResponse> => {
  const config = {
    ...defaultConfig
  };

  // Flag we are waiting
  const appStore = useAppStore();
  appStore.incrementBusy();

  try {
    const response = await axiosApi.post<TResponse>(url, requestData, config);
    return response?.data ?? ({} as TResponse);
  } catch (err: unknown) {
    const apiError = handleApiError(err, url, 'POST', errorHandlerCallback, !retrying);

    throw apiError;
  } finally {
    appStore.decrementBusy();
  }
};

export const httpPut = async <TRequest, TResponse>(
  requestData: TRequest,
  url: string,
  errorHandlerCallback?: HandleErrorCallback,
  retrying?: boolean
): Promise<TResponse> => {
  const config = {
    ...defaultConfig
  };

  // Flag we are waiting
  const appStore = useAppStore();
  appStore.incrementBusy();

  try {
    const response = await axiosApi.put<TResponse>(url, requestData, config);
    return response?.data ?? ({} as TResponse);
  } catch (err: unknown) {
    const apiError = handleApiError(err, url, 'PUT', errorHandlerCallback, !retrying);

    throw apiError;
  } finally {
    appStore.decrementBusy();
  }
};

export const httpPatch = async <TRequest, TResponse>(
  requestData: TRequest,
  url: string,
  errorHandlerCallback?: HandleErrorCallback,
  retrying?: boolean
): Promise<TResponse> => {
  const config = {
    ...defaultConfig
  };

  // Flag we are waiting
  const appStore = useAppStore();
  appStore.incrementBusy();

  try {
    const response = await axiosApi.patch<TResponse>(url, requestData, config);
    return response?.data ?? ({} as TResponse);
  } catch (err: unknown) {
    const apiError = handleApiError(err, url, 'PATCH', errorHandlerCallback, !retrying);

    throw apiError;
  } finally {
    appStore.decrementBusy();
  }
};

export const httpDelete = async (url: string, errorHandlerCallback?: HandleErrorCallback, retrying?: boolean): Promise<boolean> => {
  const config = {
    ...defaultConfig
  };

  // Flag we are waiting
  const appStore = useAppStore();
  appStore.incrementBusy();

  try {
    await axiosApi.delete(url, config);
    return true;
  } catch (err: unknown) {
    const apiError = handleApiError(err, url, 'DELETE', errorHandlerCallback, !retrying);

    throw apiError;
  } finally {
    appStore.decrementBusy();
  }
};
