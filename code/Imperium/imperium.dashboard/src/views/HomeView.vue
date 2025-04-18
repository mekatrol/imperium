<template>
  <main class="home">
    <div class="time-card">
      <p class="time">{{ timeDisplay }}</p>
      <p class="date">{{ dateDisplay }}</p>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useIntervalTimer } from '@/composables/timer';
import { getShortDateWithDay, getTimeWithMeridiem } from '@/services/date-helper';
import { ref } from 'vue';

const timeDisplay = ref('');
const dateDisplay = ref('');

const updateDateTime = (): void => {
  const dt = new Date();
  timeDisplay.value = getTimeWithMeridiem(dt);
  dateDisplay.value = getShortDateWithDay(dt);
};

useIntervalTimer(async () => {
  updateDateTime();

  // Keep timer running
  return true;
}, 1000);

updateDateTime();

</script>

<style lang="css">
:root {
  --clr-time: #ff0000;
  --clr-date: #55ff88;
}

.home {
  min-width: 800px;
  max-width: 800px;
  min-height: 480px;
  max-height: 480px;
  outline: 1px solid greenyellow;
  outline-offset: -1px;
}

.time-card {
  display: flex;
  flex-direction: column;
  gap: 3px;
  align-items: center;
  font-family: 'Orbitron';
  margin-top: 1rem;
  width: 100%;

  @font-face {
    font-family: 'Orbitron';
    font-style: normal;
    font-weight: 400 900;
    font-display: swap;
    src: url(https://fonts.gstatic.com/s/orbitron/v31/yMJRMIlzdpvBhQQL_Qq7dy0.woff2) format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+2074, U+20AC,
      U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
  }

  .time {
    font-size: 5rem;
    color: var(--clr-time);
  }

  .date {
    font-size: 1rem;
    color: var(--clr-date);
  }
}
</style>
