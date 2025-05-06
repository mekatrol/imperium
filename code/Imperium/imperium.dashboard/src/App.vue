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
import { closeWebSocket, useServerUpdateWebSocket } from './services/web-socket';
import { useIntervalTimer } from './composables/timer';

const appStore = useAppStore();
const { messageData, isBusy } = storeToRefs(appStore);

onMounted(() => {
  // Make sure websockets active
  useServerUpdateWebSocket();
});

onBeforeUnmount(() => {
  closeWebSocket();
});

const getApplicationExecutionVersion = async (): Promise<string> => {
  const appVersion = await appStore.getApplicationVersion((error) => {
    appStore.setServerOnlineStatus(false);
    return true;
  }, false);

  appStore.setServerOnlineStatus(true);
  return appVersion.executionVersion;
};

useIntervalTimer(async () => {
  // Get any updated application exectuion version
  const serverApplicationExecutionVersion = await getApplicationExecutionVersion();

  const params = new URLSearchParams(window.location.search);
  const applicationExecutionVersion = params.get('v');
  const viewName = params.get('n');

  if (serverApplicationExecutionVersion != applicationExecutionVersion) {
    // Reload the page using the new version
    const updatedUrl = location.protocol + '//' + location.host + location.pathname + `?n=${viewName ?? ''}&v=${serverApplicationExecutionVersion}`;
    window.location.replace(updatedUrl);
  }

  // Keep timer running
  return true;
}, 5000);
</script>
