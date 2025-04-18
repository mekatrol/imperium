import { AxiosError, type AxiosRequestConfig } from 'axios';
import { ref } from 'vue';
import { showErrorMessage } from './message';
import { useAppStore } from '@/stores/app-store';

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

export const apiErrorToDescription = (apiError: ApiErrorType): string => {
  switch (apiError) {
    case ApiErrorType.Undefined:
      return 'Undefined';

    case ApiErrorType.ConnectionFailed:
      return 'Connection Failed';

    case ApiErrorType.Timeout:
      return 'Timeout';

    case ApiErrorType.BadRequest:
      return 'Bad Request';

    case ApiErrorType.Unauthorized:
      return 'Unauthorized';

    case ApiErrorType.PaymentRequired:
      return 'Payment Required';

    case ApiErrorType.Forbidden:
      return 'Forbidden';

    case ApiErrorType.NotFound:
      return 'Not Found';

    case ApiErrorType.NotAllowed:
      return 'Not Allowed';

    case ApiErrorType.NotAcceptable:
      return 'Not Acceptable';

    case ApiErrorType.RequestTimeout:
      return 'Request Timeout';

    case ApiErrorType.Conflict:
      return 'Conflict';

    case ApiErrorType.Gone:
      return 'Gone';

    case ApiErrorType.InternalServerError:
      return 'Internal Server Error';

    case ApiErrorType.NotImplemented:
      return 'Not Implemented';

    case ApiErrorType.BadGateway:
      return 'Bad Gateway';

    case ApiErrorType.ServiceUnavailable:
      return 'Service Unavailable';

    case ApiErrorType.GatewayTimeout:
      return 'Gateway Timeout';

    default:
      // This is not an error code we have specifically coded for so return the
      // actual error number to help with diagnosis
      return `Error code: ${apiError}`;
  }
};

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

export const handleApiResponseError = (err: unknown): ApiError => {
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
    apiError.errors = [{ property: null, errorMessage: 'Failed to connect to the server.' }];
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
  action: string,
  errorHandlerCallback?: HandleErrorCallback,
  suppressUnauthorizedError?: boolean
): ApiError => {
  const apiError = handleApiResponseError(error);

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
    displayErrorMessage(apiError, action);
  }

  // Return error for further processing as needed
  return apiError;
};

const displayErrorMessage = (error: ApiError, action: string): void => {
  // A HTTP 409 (conflict) indicates update concurrency exception
  if (error.errorType === ApiErrorType.Conflict) {
    showErrorMessage(`${error.errors[0].errorMessage}`);
  }

  // A HTTP 404 (not found)
  else if (error.errorType === ApiErrorType.NotFound) {
    showErrorMessage(`${action} failed because the item no longer exists.`);
  } else if (error.errorType === ApiErrorType.BadRequest) {
    showErrorMessage(`${action} failed with error '${error.errors[0].errorMessage}'.`);
  } else {
    showErrorMessage(`${action} failed. Error was: '${apiErrorToDescription(error.errorType)}'.`);
  }

  // Rethrow message in case called needs info
  throw error;
};

export const wrapApiCall = async <T>(
  errorDesc: string,
  apiCall: () => Promise<T>,
  errorHandlerCallback?: HandleErrorCallback,
  showBusy: boolean = true
): Promise<T> => {
  const appStore = useAppStore();
  try {
    if (showBusy) {
      appStore.incrementBusy();
    }

    return await apiCall();
  } catch (err) {
    const apiError = handleApiError(err, errorDesc, errorHandlerCallback, false);
    throw apiError;
  } finally {
    if (showBusy) {
      appStore.decrementBusy();
    }
  }
};
