import axios from "axios";
import router from "@/router";
import { useAuthStore } from "@/stores/auth";
import type { AxiosRequestConfig } from "axios";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH";

// Map to store ongoing requests by key
const pendingRequests = new Map<string, AbortController>();

// Debug flag - set to false in production
const DEBUG_REQUESTS = import.meta.env.VITE_API_DEBUG_MODE === 'true';

function getCookie(name: string): string | null {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return decodeURIComponent(parts.pop()!.split(";")[0]);
  return null;
}

// Create an Axios instance
const axiosInstance = axios.create({
  withCredentials: true,
  withXSRFToken: true,
});

// Response interceptor for handling auth errors
axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (axios.isCancel(error)) {
      return Promise.reject(error);
    }

    if (error.response?.status === 401) {
      // Clear auth and redirect to login
      try {
        const auth = useAuthStore();
        auth.$reset();
      } catch {
        localStorage.removeItem('auth');
      }
      router.push({ name: 'login' });
    }

    return Promise.reject(error);
  }
);

export const useAPI = (baseUrl?: string) => {
  const base = baseUrl || import.meta.env.VITE_BACKEND_URL;
  const baseAPI = baseUrl || import.meta.env.VITE_API_BASE_URL;

  async function getCsrfCookie() {
    try {
      await axiosInstance.get(`${base}/sanctum/csrf-cookie`);
    } catch (error) {
      console.warn('CSRF cookie request failed (optional for token auth):', error);
    }
  }

  const useFetch = async <T>({
    url,
    requestBody,
    params,
    method = "GET",
    maximumRetries = 1,
    retryCount = 0,
    config,
    cancelPrevious = false,
  }: {
    url: string;
    maximumRetries?: number;
    retryCount?: number;
    requestBody?: Record<string, any>;
    params?: Record<string, any>;
    method?: HttpMethod;
    config?: AxiosRequestConfig;
    cancelPrevious?: boolean;
  }) => {
    const auth = useAuthStore();
    const xsrfToken = getCookie("XSRF-TOKEN");

    const fullUrl = url.includes('/sanctum') || url.includes('/broadcasting')
      ? `${base}${url}`
      : `${baseAPI}${url}`;

    // Create a unique key for this request
    const requestKey = `${method}:${fullUrl}`;

    // Cancel previous request if cancelPrevious is true
    if (cancelPrevious) {
      const existingController = pendingRequests.get(requestKey);
      if (existingController) {
        if (DEBUG_REQUESTS) {
          console.log(`🚫 Cancelling previous request: ${requestKey}`);
        }
        existingController.abort();
      }
    }

    // Create new AbortController for this request
    const abortController = new AbortController();
    pendingRequests.set(requestKey, abortController);
    if (DEBUG_REQUESTS) {
      console.log(`📤 Starting request: ${requestKey}`, `(${pendingRequests.size} pending)`);
    }

    try {
      const configurations = {
        ...config,
        signal: abortController.signal,
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
          ...(auth.token ? { Authorization: `Bearer ${auth.token}` } : {}),
          ...(xsrfToken ? { "X-XSRF-TOKEN": xsrfToken } : {}),
          ...(config?.headers ?? {}),
        },
        ...(params ? { params } : {}),
      };

      if (DEBUG_REQUESTS) {
        console.log(`[Auth Token]`, auth.token ? `Bearer ${auth.token.substring(0, 20)}...` : 'NO TOKEN');
      }

      let response;
      switch (method) {
        case "GET":
          response = await axiosInstance.get<T>(fullUrl, configurations);
          break;
        case "POST":
          await getCsrfCookie();
          response = await axiosInstance.post<T>(fullUrl, requestBody, configurations);
          break;
        case "PUT":
          await getCsrfCookie();
          response = await axiosInstance.put<T>(fullUrl, requestBody, configurations);
          break;
        case "DELETE":
          await getCsrfCookie();
          response = await axiosInstance.delete<T>(fullUrl, configurations);
          break;
        case "PATCH":
          await getCsrfCookie();
          response = await axiosInstance.patch<T>(fullUrl, requestBody, configurations);
          break;
        default:
          throw new Error(`Unsupported HTTP method: ${method}`);
      }

      if (DEBUG_REQUESTS) {
        console.log(`✅ Completed request: ${requestKey}`);
      }
      return response.data;
    } catch (error: any) {
      if (axios.isCancel(error)) {
        if (DEBUG_REQUESTS) {
          console.log(`❌ Cancelled request: ${requestKey}`);
        }
        throw error;
      }
      throw error;
    } finally {
      pendingRequests.delete(requestKey);
    }
  };

  useFetch.get = async <T>(
    url: string,
    params?: Record<string, any>,
    config?: AxiosRequestConfig,
    cancelPrevious: boolean = true
  ) => {
    return useFetch<T>({ url, params, method: "GET", config, cancelPrevious });
  };

  useFetch.post = async <T>(
    url: string,
    requestBody: Record<string, any>,
    params?: Record<string, any>,
    config?: AxiosRequestConfig,
    cancelPrevious?: boolean
  ) => {
    return useFetch<T>({ url, requestBody, params, method: "POST", config, cancelPrevious });
  };

  useFetch.put = async <T>(
    url: string,
    requestBody: Record<string, any>,
    params?: Record<string, any>,
    config?: AxiosRequestConfig,
    cancelPrevious?: boolean
  ) => {
    return useFetch<T>({ url, requestBody, params, method: "PUT", config, cancelPrevious });
  };

  useFetch.delete = async <T>(
    url: string,
    params?: Record<string, any>,
    config?: AxiosRequestConfig,
    cancelPrevious?: boolean
  ) => {
    return useFetch<T>({ url, params, method: "DELETE", config, cancelPrevious });
  };

  useFetch.patch = async <T>(
    url: string,
    requestBody: Record<string, any>,
    params?: Record<string, any>,
    config?: AxiosRequestConfig,
    cancelPrevious?: boolean
  ) => {
    return useFetch<T>({ url, requestBody, params, method: "PATCH", config, cancelPrevious });
  };

  useFetch.getCsrfCookie = getCsrfCookie;

  return useFetch;
};

// Helper function to cancel all pending requests
export const cancelAllRequests = () => {
  const count = pendingRequests.size;
  if (DEBUG_REQUESTS && count > 0) {
    console.log(`🚫 Cancelling ${count} pending request(s)`, Array.from(pendingRequests.keys()));
  }
  pendingRequests.forEach((controller) => {
    controller.abort();
  });
  pendingRequests.clear();
};

// Helper function to cancel specific request by key
export const cancelRequest = (method: HttpMethod, url: string, baseUrl?: string) => {
  const base = baseUrl || import.meta.env.VITE_BACKEND_URL;
  const baseAPI = baseUrl || import.meta.env.VITE_API_BASE_URL;

  const fullUrl = url.includes('/sanctum')
    ? `${base}${url}`
    : `${baseAPI}${url}`;

  const requestKey = `${method}:${fullUrl}`;
  const controller = pendingRequests.get(requestKey);
  if (controller) {
    controller.abort();
    pendingRequests.delete(requestKey);
    if (DEBUG_REQUESTS) {
      console.log(`🚫 Cancelled request: ${requestKey}`);
    }
  }
};

// Export a default instance for simple usage
const api = useAPI();
export default api;
