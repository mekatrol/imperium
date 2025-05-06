<template>
  <div class="time-card">
    <p class="time">{{ timeDisplay }}</p>
    <div style="display: flex; flex-direction: row; gap: 30px">
      <div
        class="sunrise"
        v-if="sunrisePoint"
      >
        <span class="material-symbols-outlined">sunny</span> {{ getTimeWithMeridiem(new Date(sunrisePoint.value! as Date), false) }}
      </div>
      <div>
        <p class="date"><span class="material-symbols-outlined">calendar_month</span>{{ dateDisplay }}</p>
      </div>
      <div
        class="sunset"
        v-if="sunsetPoint"
      >
        <span class="material-symbols-outlined">routine</span>{{ getTimeWithMeridiem(new Date(sunsetPoint.value! as Date), false) }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useIntervalTimer } from '@/composables/timer';
import { getShortDateWithDay, getTimeWithMeridiem } from '@/services/date-helper';
import { usePointStore } from '@/stores/point-store';
import { ref } from 'vue';

const pointStore = usePointStore();

const timeDisplay = ref('');
const dateDisplay = ref('');

const sunsetPoint = pointStore.getPoint('device.sunrisesunset', 'Sunset');
const sunrisePoint = pointStore.getPoint('device.sunrisesunset', 'Sunrise');

const updateDateTime = (): void => {
  const dt = new Date();
  timeDisplay.value = getTimeWithMeridiem(dt);
  dateDisplay.value = getShortDateWithDay(dt);
};

useIntervalTimer(async () => {
  // Update the date and time
  updateDateTime();

  // Keep timer running
  return true;
}, 1000);

updateDateTime();
</script>

<style lang="css" scoped>
.time-card {
  display: flex;
  flex-direction: column;
  gap: 0;
  align-items: center;
  font-family: 'Orbitron';
  width: 100%;

  @font-face {
    font-family: 'Orbitron';
    font-style: normal;
    font-weight: 400 900;
    font-display: swap;
    src: url(https://fonts.gstatic.com/s/orbitron/v31/yMJRMIlzdpvBhQQL_Qq7dy0.woff2) format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
  }

  .time {
    font-size: 5rem;
    color: var(--clr-time);
  }

  .date,
  .sunrise,
  .sunset {
    height: 100%;
    display: flex;
    flex-direction: row;
    gap: 5px;
    align-content: center;
    justify-content: center;
  }

  .date {
    font-size: 1.3rem;
    color: var(--clr-date);
  }

  .sunrise {
    font-size: 1.3rem;
    color: var(--clr-sunrise);
  }

  .sunset {
    font-size: 1.3rem;
    color: var(--clr-sunset);
  }
}
</style>
