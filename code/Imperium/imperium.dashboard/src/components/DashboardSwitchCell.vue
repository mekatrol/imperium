<template>
  <div :class="`dashboard-cell dashboard-switch-cell${cssClass ? ' ' + cssClass : ''} ${getCssClass()} `">
    <button
      @click="togglePointState"
      :disabled="isOffline()"
    >
      <div>
        <span
          v-if="icon"
          class="material-symbols-outlined"
          >{{ icon }}</span
        >
      </div>
      <div>
        <div class="spacer">
          <p>&nbsp;</p>
        </div>
        <div class="label">
          <p>{{ label }}</p>
        </div>
        <div
          class="status"
          v-if="!!countdown"
        >
          <span class="countdown">{{ countdown }}</span>
        </div>
        <div
          class="status"
          v-else
        >
          <span
            :class="`material-symbols-outlined ${getOverrideIconVisible() ? 'on' : ''}`"
            :aria-hidden="!getOverrideIconVisible()"
            >lock</span
          >
          <span
            :class="`material-symbols-outlined ${getControlIconVisible() ? 'on' : ''}`"
            :aria-hidden="!getControlIconVisible()"
            >account_tree</span
          >
          <span
            :class="`material-symbols-outlined ${getDeviceIconVisible() ? 'on' : ''}`"
            :aria-hidden="!getDeviceIconVisible()"
            >link_off</span
          >
        </div>
      </div>
    </button>
  </div>
</template>

<script setup lang="ts">
import { type Point } from '@/models/point';
import { useAppStore } from '@/stores/app-store';
import { usePointStore } from '@/stores/point-store';
import { computed, ref, type Ref } from 'vue';

interface Props {
  icon?: string;
  cssClass?: string;

  valueDeviceKey: string;
  valuePointKey: string;
  countDownDeviceKey?: string;
  countDownPointKey?: string;
}

const appStore = useAppStore();
const pointStore = usePointStore();

const props = defineProps<Props>();

const valuePoint = pointStore.getPoint(props.valueDeviceKey, props.valuePointKey, undefined);
const countdownPoint: Ref<Point | undefined> = props.countDownDeviceKey ? pointStore.getPoint(props.valueDeviceKey, props.valuePointKey, undefined) : ref(undefined);

const label = computed(() => valuePoint.value?.friendlyName ?? '<LABEL>');

const getOverrideIconVisible = (): boolean => {
  return !!(valuePoint?.value && valuePoint.value.overrideValue != null);
};

const getControlIconVisible = (): boolean => {
  return !!(valuePoint.value && valuePoint.value.controlValue != null);
};

const getDeviceIconVisible = (): boolean => {
  if (isOffline()) {
    // If the device is offline then should device icon
    return true;
  }

  if (!valuePoint.value) {
    return false;
  }

  // If neither values are defined then the device value must be correct!
  if (valuePoint.value.controlValue == undefined && !valuePoint.value.overrideValue == undefined) {
    return false;
  }

  // The current value should match the device value
  return !!(valuePoint.value && valuePoint.value.value != valuePoint.value.deviceValue);
};

const timeLeft = (countdownExpiry: Date): string => {
  const now = new Date().getTime();
  const endDate = countdownExpiry.getTime();
  const diff = endDate - now;

  const hours = `${Math.floor(diff / 3.6e6)}`.padStart(2, '0');
  const minutes = `${Math.floor((diff % 3.6e6) / 6e4)}`.padStart(2, '0');
  const seconds = `${Math.floor((diff % 6e4) / 1000)}`.padStart(2, '0');

  return `${hours}:${minutes}:${seconds}`;
};

const countdown = computed((): string | undefined => {
  if (!countdownPoint.value?.value) {
    return undefined;
  }

  const countdownExpiry = new Date(countdownPoint.value.value as Date);

  return `${timeLeft(countdownExpiry)}`;
});

const getCssClass = (): string => {
  if (valuePoint.value === undefined) {
    return 'state-offline';
  }

  return valuePoint.value.value === 1 || valuePoint.value.value === true ? 'state-on' : 'state-off';
};

const isOffline = (): boolean => {
  return valuePoint.value?.value === null;
};

const togglePointState = async (): Promise<void> => {
  if (!valuePoint.value.deviceKey) {
    return;
  }

  try {
    await appStore.togglePointState(valuePoint.value.deviceKey, valuePoint.value.key);

    appStore.setServerOnlineStatus(true);
  } catch {
    appStore.setServerOnlineStatus(false);
  }
};
</script>

<style lang="css" scoped>
/* Button layout */
.dashboard-switch-cell button {
  display: flex;
  flex-direction: row;
}

/* Button sections (icon + text) */
.dashboard-switch-cell button > div {
  display: flex;
  flex-direction: column;
  align-content: center;
  justify-content: center;
}

/* Icon section */
.dashboard-switch-cell button > :first-child {
  width: 35%;
  display: flex;
}

/* Text section */
.dashboard-switch-cell button > :not(:first-child) {
  width: 65%;
  display: flex;
  flex-direction: column;
  padding: 0;
  margin: 0;
}

/* Text section children */
.dashboard-switch-cell button > :not(:first-child) > * {
  padding: 0;
  margin: 0;
  line-height: 1rem;
}

.dashboard-switch-cell button .label {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.dashboard-switch-cell button .label > p {
  line-height: 1rem;
  font-size: 1rem;
  text-align: center;
  align-self: center;
}

.dashboard-switch-cell button .status {
  display: flex;
  flex-direction: row;
}

.dashboard-switch-cell button .status > span:not(.countdown) {
  display: flex;
  line-height: 1.1rem;
  font-size: 1.1rem;
  text-align: right;
  width: 33%;

  /* Effectively hide the icon */
  color: transparent;
}

.dashboard-switch-cell button .status > span.countdown {
  display: flex;
  line-height: 0.8rem;
  font-size: 0.8rem;
  color: var(--clr-state-off);
}

.dashboard-switch-cell.state-off > button:active .status > span {
  color: var(--clr-state-off);
}

.dashboard-switch-cell.state-on > button:active .status > span {
  color: var(--clr-state-on);
}

.dashboard-switch-cell.state-off button .status > span.on {
  color: var(--clr-state-off);
}

.dashboard-switch-cell.state-on button .status > span.on {
  color: var(--clr-state-on);
}
</style>
