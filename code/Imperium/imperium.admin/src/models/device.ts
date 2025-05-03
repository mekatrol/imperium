export interface Device {
  key: string;
  controllerKey: string;
  enabled: boolean;
  online: boolean;
  lastCommunication?: Date;
}
