import { describe, expect, it } from 'vitest';
import { friendlyCodeToNumberCode, htmlEncodings } from '@/services/html-encodings';

describe('unicode hex value matches unicode of value property', () => {
  htmlEncodings
    .filter((e) => e.value.length === 1)
    .forEach((e) => {
      it(`encoding unicode check ${e.value}`, () => {
        expect(e.value.charCodeAt(0)).toBe(e.unicode);
      });
    });
});

describe('unicode escaped value equals friendly name escaped value', () => {
  it('&#tab;', () => {
    expect(friendlyCodeToNumberCode('&#tab;')).toBe('&#9;');
  });
  it('&#newline;', () => {
    expect(friendlyCodeToNumberCode('&#newline;')).toBe('&#10;');
  });
  it('&#nbsp;', () => {
    expect(friendlyCodeToNumberCode('&#nbsp;')).toBe('&#32;');
  });
  it('&#quot;', () => {
    expect(friendlyCodeToNumberCode('&#quot;')).toBe('&#34;');
  });
  it('&#amp;', () => {
    expect(friendlyCodeToNumberCode('&#amp;')).toBe('&#38;');
  });
  it('&#lt;', () => {
    expect(friendlyCodeToNumberCode('&#lt;')).toBe('&#60;');
  });
  it('&#gt;', () => {
    expect(friendlyCodeToNumberCode('&#gt;')).toBe('&#62;');
  });
});
