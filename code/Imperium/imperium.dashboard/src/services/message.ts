import { useAppStore } from '@/stores/app-store';

export enum MessageType {
  // Blue message with 'i' icon
  Info = 'Info',

  // Green message with tick icon
  Success = 'Success',

  // Orange-ish message with exclamation icon
  Warn = 'Warn',

  // Red message with 'X' icon
  Error = 'Error'
}

export interface MessageData {
  type: MessageType;
  title: string;
  message: string;
}

const showMessage = (type: MessageType, title: string, message: string): void => {
  const appStore = useAppStore();

  appStore.messageData = {
    message: message,
    title: title,
    type: type
  };
};

export const clearMessage = (): void => {
  const appStore = useAppStore();
  appStore.messageData = undefined;
};

export const showErrorMessage = (message: string): void => {
  showMessage(MessageType.Error, 'Error', message);
};

export const showWarningMessage = (message: string): void => {
  showMessage(MessageType.Warn, 'Warning', message);
};

export const showSuccessMessage = (message: string): void => {
  showMessage(MessageType.Success, 'Success', message);
};

export const showInfoMessage = (message: string): void => {
  showMessage(MessageType.Info, 'Information', message);
};
