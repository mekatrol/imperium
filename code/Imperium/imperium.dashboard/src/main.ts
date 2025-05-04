import './assets/main.css';

import { createApp } from 'vue';
import { createPinia } from 'pinia';

import App from './App.vue';
import router from './router';
import { closeWebSocket, useServerUpdateWebSocket } from '@/services/ws';

// Make sure websockets active
const ws = useServerUpdateWebSocket();

// Close web sockets if user navigates away
window.addEventListener('beforeunload', () => {
  if (ws && ws.readyState === WebSocket.OPEN) {
    closeWebSocket(ws);
  }
});

const app = createApp(App);

app.use(createPinia());
app.use(router);

app.mount('#app');
