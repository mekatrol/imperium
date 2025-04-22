import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { clearMessage, type MessageData } from '@/services/message';
import { type HandleErrorCallback } from '@/services/api';
import type { Point, PointTypes, PointValueUpdate } from '@/models/point';
import { httpGet, httpPost } from '@/services/http';

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

  const getPoints = async (errorHandlerCallback?: HandleErrorCallback, showBusy: boolean = true): Promise<Point[]> => {
    return await httpGet<Point[]>('/points', errorHandlerCallback, false, showBusy);
  };

  const updatePoint = async (id: string, value: PointTypes, errorHandlerCallback?: HandleErrorCallback): Promise<Point> => {
    const model: PointValueUpdate = {
      id: id,
      value: value === undefined || value === null ? null : `${value}`
    };

    return await httpPost<PointValueUpdate, Point>(model, '/points', errorHandlerCallback, false);
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

    getPoints,
    updatePoint
  };
});
