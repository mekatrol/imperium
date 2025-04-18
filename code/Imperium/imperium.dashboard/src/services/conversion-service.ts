export const stringToBoolean = (booleanAsString?: string | undefined | null): boolean => {
  return !!booleanAsString && (booleanAsString.toLowerCase() === 'true' || booleanAsString === '1');
};

export const booleanToString = (booleanValue?: boolean | undefined | null): string => {
  return booleanValue === true ? 'true' : 'false';
};
