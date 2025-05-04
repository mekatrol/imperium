import { getWebSocketBaseUrl } from './url';

const url = getWebSocketBaseUrl();
const ws = new WebSocket(url);

ws.onopen = (): void => {
  console.log('WebSocket connected');
  // Simulate authentication or subscription
  ws.send(
    JSON.stringify({
      subscriptionType: 'valueChange'
    })
  );
};

ws.onmessage = (event): void => {
  const data = JSON.parse(event.data);
  console.log('Event received:', data);

  if (data.type === 'event' && data.event_type === 'stateChanged') {
    const { entity_id, new_state } = data.data;
    console.log(`Entity ${entity_id} changed to: ${new_state.state}`);
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
