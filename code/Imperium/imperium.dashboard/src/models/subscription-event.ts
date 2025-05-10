import type { PointTypes } from './point';

export enum EventType {
  ValueChange = 'ValueChange',
  Refresh = 'Refresh'
}

export enum EntityType {
  Device = 'Device',
  Point = 'Point'
}

export interface SubscriptionEvent {
  eventType: EventType;
  entityType: EntityType;
  deviceKey: string;
  pointKey: string;
  value: PointTypes;
}
