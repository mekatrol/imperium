import { EntityType } from '@/models/subscription-event';
import { getWebSocketBaseUrl } from './url';
import { usePointStore } from '@/stores/point-store';
import { ref, type Ref } from 'vue';

const url = getWebSocketBaseUrl();
const webSocket = ref<WebSocket | undefined>(undefined);

const initWebSocket = (ws: WebSocket): void => {
  webSocket.value = ws;

  const reconnect = (): void => {
    const timerHandle = window.setInterval(async (): Promise<void> => {
      if (webSocket.value && webSocket.value.readyState === WebSocket.OPEN) {
        // Socket is connected, so stop timer
        return;
      }

      // Try connecting
      const ws = new WebSocket(url);
      initWebSocket(ws);

      // Stop timer
      clearInterval(timerHandle);
    }, 5000);
  };

  webSocket.value.onopen = (): void => {
    // Subscribe to value changes
    ws.send(
      JSON.stringify({
        subscriptionType: 'valueChange'
      })
    );
  };

  webSocket.value.onmessage = (event): void => {
    const pointStore = usePointStore();

    const data = JSON.parse(event.data);

    if (data.entityType === EntityType.point) {
      pointStore.updatePoint(data.point.deviceKey, data.point.key, data.point);
    }
  };

  webSocket.value.onclose = (): void => {
    reconnect();
  };

  webSocket.value.onerror = (_error): void => {
    reconnect();
  };
};

export const closeWebSocket = (code = 1000, reason = 'Manual close'): void => {
  if (webSocket.value && webSocket.value.readyState === WebSocket.OPEN) {
    webSocket.value.close(code, reason);
  }
};

export const useServerUpdateWebSocket = (): Ref<WebSocket | undefined> => {
  initWebSocket(new WebSocket(url));

  return webSocket;
};
