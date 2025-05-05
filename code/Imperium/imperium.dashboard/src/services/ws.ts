import { EntityType } from '@/models/subscription-event';
import { getWebSocketBaseUrl } from './url';
import { usePointStore } from '@/stores/point-store';

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
  const pointStore = usePointStore();

  const data = JSON.parse(event.data);

  if (data.entityType === EntityType.point) {
    pointStore.addOrUpdatePoint(data.point);
  }
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
