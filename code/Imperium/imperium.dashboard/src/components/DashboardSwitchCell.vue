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
import { type CountdownPoint } from '@/models/point';
import { useAppStore } from '@/stores/app-store';
import { computed, type Ref } from 'vue';

interface Props {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
  state?: string | undefined;
}

const appStore = useAppStore();

const model = defineModel<Ref<CountdownPoint>>();
defineProps<Props>();

const getOverrideIconVisible = (): boolean => {
  return !!(model.value?.value && model.value.value.valuePoint.overrideValue != null);
};

const getControlIconVisible = (): boolean => {
  return !!(model.value?.value && model.value.value.valuePoint.controlValue != null);
};

const getDeviceIconVisible = (): boolean => {
  if (isOffline()) {
    // If the device is offline then should device icon
    return true;
  }

  if (!model.value?.value?.valuePoint) {
    return false;
  }

  // If neither values are defined then the device value must be correct!
  if (model.value.value.valuePoint.controlValue == undefined && !model.value.value.valuePoint.overrideValue == undefined) {
    return false;
  }

  // The current value should match the device value
  return !!(model.value?.value && model.value.value.valuePoint.value != model.value.value.valuePoint.deviceValue);
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
  if (!model.value?.value?.countdownPoint?.value) {
    return undefined;
  }

  const countdownExpiry = new Date(model.value.value.countdownPoint.value as Date);

  return `${timeLeft(countdownExpiry)}`;
});

const getCssClass = (): string => {
  if (model.value?.value === undefined) {
    return 'state-offline';
  }

  return model.value.value.valuePoint.value === 1 || model.value.value.valuePoint.value === true ? 'state-on' : 'state-off';
};

const isOffline = (): boolean => {
  return model.value?.value?.valuePoint.value === null;
};

const togglePointState = async (): Promise<void> => {
  if (!model.value?.value?.valuePoint.deviceKey) {
    return;
  }

  try {
    await appStore.togglePointState(model.value.value.valuePoint.deviceKey, model.value.value.valuePoint.key);

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
