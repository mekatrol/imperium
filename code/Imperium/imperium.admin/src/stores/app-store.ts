import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { clearMessage, type MessageData } from '@/services/message';
import { type HandleErrorCallback } from '@/services/api';
import { httpGet } from '@/services/http';
import type { Device } from '@/models/device';

export const useAppStore = defineStore('app', () => {
  const isBusyCount = ref(0);
  const messageData = ref<MessageData | undefined>(undefined);
  const serverOnline = ref(false);

  const isBusy = computed(() => isBusyCount.value > 0);

  const closeMessageOverlay = (): void => {
    clearMessage();
  };

  const incrementBusy = (): void => {
    isBusyCount.value++;
  };

  const decrementBusy = (): void => {
    isBusyCount.value--;

    if (isBusyCount.value < 0) {
      isBusyCount.value = 0;
    }
  };

  const getDevices = async (errorHandlerCallback?: HandleErrorCallback, showBusy: boolean = true): Promise<Device[]> => {
    return await httpGet<Device[]>('/devices', errorHandlerCallback, false, showBusy);
  };

  const setServerOnlineStatus = (status: boolean): void => {
    serverOnline.value = status;
  };

  return {
    messageData,
    closeMessageOverlay,
    isBusy,
    incrementBusy,
    decrementBusy,

    serverOnline,
    setServerOnlineStatus,

    getDevices
  };
});
