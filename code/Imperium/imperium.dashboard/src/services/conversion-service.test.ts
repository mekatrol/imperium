import { describe, expect, it } from 'vitest';
import { stringToBoolean, booleanToString } from '@/services/conversion-service';

describe('stringToBoolean', () => {
  it('no string returns false', () => {
    expect(stringToBoolean()).toBe(false);
  });

  it('empty string returns false', () => {
    expect(stringToBoolean('')).toBe(false);
  });

  it('undefined string returns false', () => {
    expect(stringToBoolean(undefined)).toBe(false);
  });

  it('null string returns false', () => {
    expect(stringToBoolean(null)).toBe(false);
  });

  it('false string returns false', () => {
    expect(stringToBoolean('false')).toBe(false);
  });

  it('False string returns false', () => {
    expect(stringToBoolean('false')).toBe(false);
  });

  it('FALSE string returns false', () => {
    expect(stringToBoolean('FALSE')).toBe(false);
  });

  it('true string returns true', () => {
    expect(stringToBoolean('true')).toBe(true);
  });

  it('True string returns true', () => {
    expect(stringToBoolean('True')).toBe(true);
  });

  it('TRUE string returns true', () => {
    expect(stringToBoolean('TRUE')).toBe(true);
  });

  it('1 string returns true', () => {
    expect(stringToBoolean('1')).toBe(true);
  });
});

describe('booleanToString', () => {
  it('false string returns false', () => {
    expect(booleanToString(false)).toBe('false');
  });

  it('no value returns false', () => {
    expect(booleanToString()).toBe('false');
  });

  it('undefined returns false', () => {
    expect(booleanToString(undefined)).toBe('false');
  });

  it('null returns false', () => {
    expect(booleanToString(null)).toBe('false');
  });

  it('true returns true', () => {
    expect(booleanToString(true)).toBe('true');
  });
});
