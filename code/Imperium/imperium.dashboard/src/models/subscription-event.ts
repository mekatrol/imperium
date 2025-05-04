import type { PointTypes } from './point';

export enum EventType {
  valueChange = 'ValueChange',
  refresh = 'Refresh'
}

export enum EntityType {
  device = 'Device',
  point = 'Point'
}

export interface SubscriptionEvent {
  eventType: EventType;
  entityType: EntityType;
  deviceKey: string;
  pointKey: string;
  value: PointTypes;
}
