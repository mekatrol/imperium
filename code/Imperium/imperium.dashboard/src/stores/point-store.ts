import { type Point } from '@/models/point';
import { defineStore } from 'pinia';
import { ref, type Ref } from 'vue';

const createDevicePointKey = (deviceKey: string, pointKey: string): string => {
  return `${deviceKey}.${pointKey}`;
};

export const usePointStore = defineStore('point', () => {
  const points: Record<string, Ref<Point | undefined>> = {};

  const updatePoint = (deviceKey: string, pointKey: string, point: Point | undefined): Ref<Point | undefined> => {
    const key = createDevicePointKey(deviceKey, pointKey);

    const exists = key in points;

    if (!exists) {
      points[key] = ref<Point | undefined>(point);
    } else {
      points[key].value = point;
    }

    return points[key];
  };

  const getPoint = (deviceKey: string, pointKey: string): Ref<Point | undefined> => {
    const key = createDevicePointKey(deviceKey, pointKey);

    // If the key does not yet exist then add it as undefined point
    const exists = key in points;
    if (!exists) {
      return updatePoint(deviceKey, pointKey, undefined);
    }

    return points[key];
  };

  return {
    getPoint,
    updatePoint
  };
});
