<template>
  <div :class="`dashboard-cell${cssClass ? ' ' + cssClass : ''} ${getCssClass()} `">
    <button @click="toggleValue" :disabled="isOffline()">
      <div><span v-if="icon" class="material-symbols-outlined">{{ icon }}</span></div>
      <div>
        <div class="spacer">
          <p>&nbsp;</p>
        </div>
        <div class="label">
          <p>{{ label }}</p>
        </div>
        <div class="status">
          <span
            v-if="model?.value && model.value.overrideValue != null && !!model.value.overrideValue != model.value.controlValue"
            class="material-symbols-outlined">lock</span>
          <span v-else></span>
          <span></span>
          <span v-if="model?.value && !!model.value.deviceValue != model.value.value"
            class="material-symbols-outlined">link_off</span>
        </div>
      </div>
    </button>
  </div>
</template>

<script setup lang="ts">
import { PointType, type Point } from '@/models/point';
import { useAppStore } from '@/stores/app-store';
import type { Ref } from 'vue';

interface Props {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
  state?: string | undefined;
}

const appStore = useAppStore();

const model = defineModel<Ref<Point>>();
defineProps<Props>();

const getCssClass = (): string => {
  if (model.value?.value === undefined) {
    return 'state-offline';
  }

  return (model.value.value.value === 1 || model.value.value.value === true) ? 'state-on' : 'state-off';
};

const isOffline = (): boolean => {
  return model.value?.value === undefined;
};

const toggleValue = async (): Promise<void> => {
  if (!model.value?.value?.id) {
    return;
  }

  try {
    let value: number | boolean | undefined = 0;
    if (model.value.value.pointType === PointType.Boolean) {
      value = model.value.value.value === true ? false : true;
    } else {
      value = model.value.value.value === 1 ? undefined : 1;
    }

    await appStore.updatePoint(model.value.value.id, value);
    appStore.setServerOnlineStatus(true);
  } catch {
    appStore.setServerOnlineStatus(false);
  }
};

</script>

<style lang="css" scoped>
/* Button layout */
.dashboard-cell button {
  display: flex;
  flex-direction: row;
}

/* Button sections (icon + text) */
.dashboard-cell button>div {
  display: flex;
  flex-direction: column;
  align-content: center;
  justify-content: center;
}

/* Icon section */
.dashboard-cell button> :first-child {
  width: 40%;
  display: flex;
}

/* Text section */
.dashboard-cell button> :not(:first-child) {
  width: 60%;
  display: flex;
  flex-direction: column;
  padding: 0;
  margin: 0;
}

/* Text section children */
.dashboard-cell button> :not(:first-child)>* {
  padding: 0;
  margin: 0;
  line-height: 1rem;
}

.dashboard-cell button .label {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.dashboard-cell button .label>p {
  line-height: 2rem;
  font-size: 1rem;
  text-align: center;
  align-self: center;
}

.dashboard-cell button .status {
  display: flex;
  flex-direction: row;
}

.dashboard-cell button .status>span {
  display: flex;
  line-height: 1.1rem;
  font-size: 1.1rem;
  text-align: right;
  width: 33%;
  /* color: var(--clr-state-offline) */
}
</style>
