<template>
  <BusyOverlay
    :show="isBusy"
    full-screen
  />
  <MessageOverlay
    :show="!!messageData"
    :data="messageData"
    full-screen
  />
  <RouterView />
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { RouterView } from 'vue-router';
import { useAppStore } from '@/stores/app-store';
import BusyOverlay from '@/components/BusyOverlay.vue';
import MessageOverlay from '@/components/MessageOverlay.vue';
import { onBeforeUnmount, onMounted } from 'vue';
import { closeWebSocket, useServerUpdateWebSocket } from './services/ws';

const { messageData, isBusy } = storeToRefs(useAppStore());

let ws: WebSocket | undefined = undefined;

onMounted(() => {
  // Make sure websockets active
  ws = useServerUpdateWebSocket();
});

onBeforeUnmount(() => {
  if (ws && ws.readyState === WebSocket.OPEN) {
    closeWebSocket(ws);
  }
});
</script>
