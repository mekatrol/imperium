export const combineUrl = (baseUrl: string, relativeUrl: string): string => {
  return relativeUrl ? baseUrl.replace(/\/+$/, '') + '/' + relativeUrl.replace(/^\/+/, '') : baseUrl;
};

export const combinePathWithBaseUrl = (relativeUrl: string): string => {
  return combineUrl(getApiBaseUrl(), relativeUrl);
};

// This will ensure the provided path is relative (ie it will strip any scheme, host, port, etc)
export const ensureRelativeUrl = (url: string): string => {
  try {
    // Try and construct a URL (and include current base URL in case is already relative)
    const absUrl = new URL(url, window.location.origin);

    // Construct relative from abs parts
    const relUrl = `${absUrl.pathname}${absUrl.search}`;

    // Return the bits after the absolute URL part
    return relUrl;
  } catch {
    // A dodgy URL was provided so return root relative path
    return '/';
  }
};

export const getApiBaseUrl = (): string => {
  /*
   * Get API base URL from hidden input in index.html, the hidden input is injected by
   * the server so that it does not need to be compiled in to the SPA.
   * This allows the server to inject the API URL when the SPA is loading on the server.
   * However, during development it will the value set in the local index.html
   */

  const baseUrl = (document.getElementById('api-base-url') as HTMLInputElement)?.value ?? '/';
  return baseUrl;
};
