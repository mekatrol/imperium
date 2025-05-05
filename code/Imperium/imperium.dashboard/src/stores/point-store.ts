import { PointState, PointType, type Point } from '@/models/point';
import { defineStore } from 'pinia';
import { ref, type Ref } from 'vue';

const createDevicePointKeyFromPoint = (point: Point): string => {
  return createDevicePointKey(point.deviceKey, point.key);
};

const createDevicePointKey = (deviceKey: string, pointKey: string): string => {
  return `${deviceKey}.${pointKey}`;
};

export const usePointStore = defineStore('point', () => {
  const points: Record<string, Ref<Point>> = {};

  const initialisePointPoint = (deviceKey: string, pointKey: string, pointType: PointType = PointType.Integer): Ref<Point> => {
    const point: Point = {
      deviceKey: deviceKey,
      key: pointKey,
      pointType: pointType,
      pointState: PointState.Offline,
      readonly: true
    };

    return addOrUpdatePoint(point);
  };

  const addOrUpdatePoint = (point: Point): Ref<Point> => {
    const key = createDevicePointKeyFromPoint(point);

    if (points[key] === undefined) {
      points[key] = ref(point);
    } else {
      points[key].value = point;
    }

    return points[key];
  };

  const getPoint = (deviceKey: string, pointKey: string): Ref<Point> | undefined => {
    return points[createDevicePointKey(deviceKey, pointKey)];
  };

  return {
    getPoint,
    initialisePointPoint,
    addOrUpdatePoint
  };
});
