import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { clearMessage, type MessageData } from '@/services/message';
import { type HandleErrorCallback } from '@/services/api';
import type { Point } from '@/models/point';
import { httpGet } from '@/services/http';

export const useAppStore = defineStore('app', () => {
  const isBusyCount = ref(0);
  const messageData = ref<MessageData | undefined>(undefined);

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

  const getPoints = async (errorHandlerCallback?: HandleErrorCallback, showBusy: boolean = true): Promise<Point[]> => {
    return await httpGet<Point[]>('/points', errorHandlerCallback, false, showBusy);
  };

  return {
    messageData,
    closeMessageOverlay,
    isBusy,
    incrementBusy,
    decrementBusy,

    getPoints
  };
});
