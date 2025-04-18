import type { AxiosError, AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import axios from 'axios';
import { getApiBaseUrl } from './url';

export const requestInterceptor = (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
  return config;
};

export const responseInterceptor = (response: AxiosResponse): AxiosResponse => {
  return response;
};

export const errorInterceptor = (error: AxiosError): Promise<unknown> => {
  return Promise.reject(error);
};

const apiBaseUrl = getApiBaseUrl();

const axiosApi: AxiosInstance = axios.create({ baseURL: apiBaseUrl });

// Configure interceptors
axiosApi.interceptors.request.use(requestInterceptor, errorInterceptor);
axiosApi.interceptors.response.use(responseInterceptor, errorInterceptor);

// Export configured axiosApi
export { axiosApi };
