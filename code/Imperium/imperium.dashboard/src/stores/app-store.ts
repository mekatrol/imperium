import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { clearMessage, type MessageData } from '@/services/message';
import { type HandleErrorCallback } from '@/services/api';
import { PointUpdateAction, type Point, type PointTypes, type PointValueUpdate } from '@/models/point';
import { httpGet, httpPost } from '@/services/http';
import type { ApplicationVersion } from '@/models/application-version';
import { FifoQueue } from '@/utils/fifi-queue';
import { type SubscriptionEvent } from '@/models/subscription-event';
import type { Dashboard } from '@/models/dashboard';

export const useAppStore = defineStore('app', () => {
  const isBusyCount = ref(0);
  const messageData = ref<MessageData | undefined>(undefined);
  const serverOnline = ref(false);
  const subscriptionEvents = ref<FifoQueue<SubscriptionEvent>>(new FifoQueue<SubscriptionEvent>());
  const dashboard = ref<Dashboard | undefined>();

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

  const getApplicationVersion = async (errorHandlerCallback?: HandleErrorCallback, showBusy: boolean = true): Promise<ApplicationVersion> => {
    return await httpGet<ApplicationVersion>('/app/version', errorHandlerCallback, false, showBusy);
  };

  const updateDashboard = async (errorHandlerCallback?: HandleErrorCallback, showBusy: boolean = true): Promise<Dashboard> => {
    dashboard.value = await httpGet<Dashboard>('/dashboards/main', errorHandlerCallback, false, showBusy);

    if (dashboard.value) {
      dashboard.value.items.forEach((item) => {
        if (item.props?.deviceKey && item.props.deviceKey === 'browser') {
          item.props.state = (): boolean => {
            return true;
          };
        }
      });
    }

    return dashboard.value;
  };

  const getPoints = async (errorHandlerCallback?: HandleErrorCallback, showBusy: boolean = true): Promise<Point[]> => {
    return await httpGet<Point[]>('/points', errorHandlerCallback, false, showBusy);
  };

  const togglePointState = async (deviceKey: string, pointKey: string, errorHandlerCallback?: HandleErrorCallback): Promise<Point> => {
    const model: PointValueUpdate = {
      deviceKey: deviceKey,
      pointKey: pointKey,
      pointUpdateAction: PointUpdateAction.Toggle,
      value: null
    };

    return await httpPost<PointValueUpdate, Point>(model, '/points', errorHandlerCallback, false);
  };

  const updatePoint = async (deviceKey: string, pointKey: string, value: PointTypes, updateAction: PointUpdateAction, errorHandlerCallback?: HandleErrorCallback): Promise<Point> => {
    const model: PointValueUpdate = {
      deviceKey: deviceKey,
      pointKey: pointKey,
      pointUpdateAction: updateAction,
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

    getApplicationVersion,

    dashboard,
    updateDashboard,

    getPoints,
    togglePointState,
    updatePoint,

    subscriptionEvents
  };
});
