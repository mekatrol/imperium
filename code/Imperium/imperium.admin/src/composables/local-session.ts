import { stringToBoolean } from '@/services/conversion-service';

export interface LocalSessionValue<T> {
  setting: T | null;
  remove(): void;
}

abstract class BaseLocalSessionValue<T> implements LocalSessionValue<T> {
  protected key: string;

  constructor(key: string) {
    this.key = key;
  }

  public get setting(): T | null {
    const value = localStorage.getItem(this.key);

    // If there was not a local storage value then return null
    if (value === null || value === undefined) {
      return null;
    }

    // Get string value as type
    const valueAsType = this.getValue(value);

    // Return value as type
    return valueAsType;
  }

  public set setting(value: T | null) {
    if (value === undefined || value == null) {
      this.remove();
      return;
    }

    this.setValue(value);
  }

  remove = (): void => {
    // Remove item from local storage
    localStorage.removeItem(this.key);
  };

  protected abstract getValue(value: string): T | null;
  protected abstract setValue(value: T): void;
}

class BoolLocalSessionValue extends BaseLocalSessionValue<boolean> {
  getValue(value: string): boolean | null {
    if (value.toLowerCase() !== 'true' && value.toLowerCase() !== 'false' && value !== '1' && value !== '0') {
      return null;
    }

    // Return the string value casted to a boolean
    return stringToBoolean(value);
  }

  setValue(value: boolean): void {
    // Update local storage
    localStorage.setItem(this.key, value ? 'true' : 'false');
  }
}

class IntLocalSessionValue extends BaseLocalSessionValue<number> {
  getValue(value: string): number | null {
    // Parse to integer
    const intValue = parseInt(value, 10);

    // Invalid integer returns as null value
    if (isNaN(intValue)) {
      return null;
    }

    return intValue;
  }

  setValue(value: number): void {
    // Update local storage
    localStorage.setItem(this.key, value.toString());
  }
}

class StringLocalSessionValue extends BaseLocalSessionValue<string> {
  getValue(value: string): string | null {
    // The value is already a string so no need to do anything
    return value;
  }

  setValue(value: string): void {
    // Update local storage
    localStorage.setItem(this.key, value);
  }
}

class JsonObjectLocalSessionValue<T> extends BaseLocalSessionValue<T> {
  getValue(value: string): T | null {
    // No value set then return null
    if (!value || value.length === 0) {
      return null;
    }

    // Convert from JSON to value and return
    return JSON.parse(value) as T;
  }

  setValue(value: T): void {
    // Convert to JSON string and store
    localStorage.setItem(this.key, JSON.stringify(value));
  }
}

export const useLocalSessionJsonObject = <T>(key: string): LocalSessionValue<T> => new JsonObjectLocalSessionValue<T>(key);
export const useLocalSessionString = (key: string): LocalSessionValue<string> => new StringLocalSessionValue(key);
export const useLocalSessionBool = (key: string): LocalSessionValue<boolean> => new BoolLocalSessionValue(key);
export const useLocalSessionInt = (key: string): LocalSessionValue<number> => new IntLocalSessionValue(key);
