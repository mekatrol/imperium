export const daysOfWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
export const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

export const getMeridiem = (dt: Date): string => {
  return dt.getHours() >= 12 ? 'PM' : 'AM';
};

export const getHours12Hour = (dt: Date): number => {
  const hrs24 = dt.getHours();
  return hrs24 > 12 ? hrs24 - 12 : hrs24;
};

export const getZeroPadded = (v: number): string => {
  return `${v}`.padStart(2, '0');
};

export const getTimeWithMeridiem = (dateTime: Date | undefined = undefined, withSeconds?: boolean): string => {
  const dt = dateTime ?? new Date();
  return getHours12Hour(dt) + ':' + getZeroPadded(dt.getMinutes()) + (withSeconds ? `:${getZeroPadded(dt.getSeconds())}` : '') + ' ' + getMeridiem(dt);
};

export const getShortDateWithDay = (dateTime: Date | undefined = undefined): string => {
  const dt = dateTime ?? new Date();
  return daysOfWeek[dt.getDay()].toLocaleUpperCase() + ' ' + dt.getDate() + ' ' + months[dt.getMonth()].toLocaleUpperCase() + ' ' + dt.getFullYear();
};
