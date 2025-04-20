<template>
  <div :class="`dashboard-cell${cssClass ? ' ' + cssClass : ''} ${getCssClass()} `">
    <button>
      <div><span v-if="icon" class="material-symbols-outlined">{{ icon }}</span></div>
      <div :class="`label ${getCountdown() ? 'counting' : ''}`">
        <p>{{ label }}</p>
        <p v-if="getCountdown()">{{ getCountdown() }}</p>
      </div>
    </button>
  </div>
</template>

<script setup lang="ts">
import type { CountdownPoint } from '@/models/point';
import type { Ref } from 'vue';

interface Props {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
  state?: string | undefined;
}

const model = defineModel<Ref<CountdownPoint>>();
defineProps<Props>();


const timeLeft = (countdownExpiry: Date): string => {
  const now = new Date().getTime();
  const endDate = countdownExpiry.getTime();
  const diff = endDate - now;

  const hours = `${Math.floor(diff / 3.6e6)}`.padStart(2, '0');
  const minutes = `${Math.floor((diff % 3.6e6) / 6e4)}`.padStart(2, '0');
  const seconds = `${Math.floor((diff % 6e4) / 1000)}`.padStart(2, '0');

  return `${hours}:${minutes}:${seconds}`;
};

const getCountdown = (): string | undefined => {
  if (!model.value?.value?.countdownPoint.value) {
    return undefined;
  }

  const countdownExpiry = new Date(model.value.value.countdownPoint.value as Date);

  return `[${timeLeft(countdownExpiry)}]`;
};

const getCssClass = (): string => {
  if (model.value?.value?.valuePoint?.value === undefined) {
    return 'state-offline';
  }

  return (model.value.value.valuePoint.value === 1 || model.value.value.valuePoint.value === true) ? 'state-on' : 'state-off';
};
</script>

<style lang="css">
.dashboard-cell>button>div {
  height: 100%;
  display: flex;
  justify-content: center;
}

.dashboard-cell>button>div>span {
  margin: auto;
  font-size: 3rem;
  align-self: flex-start;
}

.dashboard-cell>button>div>p {
  margin: auto;
  font-size: 1rem;
}

.label {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.label.counting>p:first-child {
  margin: auto;
  line-height: 0.5rem;
}

.label.counting>p:not(:first-child) {
  margin: auto;
  line-height: 0.3rem;
  font-size: 0.8rem;
}
</style>
