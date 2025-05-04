import { useAppStore } from '@/stores/app-store';
import { getWebSocketBaseUrl } from './url';

const url = getWebSocketBaseUrl();
const ws = new WebSocket(url);

ws.onopen = (): void => {
  // Subscribe to value changes
  ws.send(
    JSON.stringify({
      subscriptionType: 'valueChange'
    })
  );
};

ws.onmessage = (event): void => {
  const { subscriptionEvents } = useAppStore();

  const data = JSON.parse(event.data);
  subscriptionEvents.enqueue(data);
};

ws.onclose = (e): void => {
  console.log('WebSocket closed', e);
};

ws.onerror = (error): void => {
  console.error('WebSocket error:', error);
};

export const closeWebSocket = (webSocket: WebSocket, code = 1000, reason = 'Manual close'): void => {
  if (webSocket && webSocket.readyState === WebSocket.OPEN) {
    webSocket.close(code, reason);
  }
};

export const useServerUpdateWebSocket = (): WebSocket => {
  return ws;
};
