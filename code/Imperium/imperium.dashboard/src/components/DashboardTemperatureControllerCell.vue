<template>
  <div :class="`dashboard-cell dashboard-temperature-controller-cell ${getCssClass()}`">
    <div>
      <div class="icon">
        <span
          v-if="icon"
          class="material-symbols-outlined"
          >{{ icon }}</span
        >
      </div>
      <div class="values">
        <div class="label">
          <span class="material-symbols-outlined">{{ enabled }}</span>
          <p>{{ label }}</p>
        </div>
        <div class="values-main">
          <span>{{ tempValue }}°C</span>
        </div>
        <div class="values-sub">
          <span>{{ spValue }}°C</span>
          <span>{{ pbValue }}°C</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { type TemperatureControlPoint } from '@/models/point';
import { computed, type Ref } from 'vue';

interface Props {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
  state?: string | undefined;
}

const model = defineModel<Ref<TemperatureControlPoint>>();
defineProps<Props>();

const getCssClass = (): string => {
  if (model.value?.value?.onPoint === undefined || model.value?.value?.enabledPoint === undefined) {
    return 'state-offline';
  }

  if (model.value?.value?.enabledPoint.value !== true) {
    return 'state-disabled';
  }

  return model.value.value.onPoint.value === true ? 'heat-on' : 'heat-off';
};

const enabled = computed(() => {
  if (!model?.value?.value?.enabledPoint?.value) {
    return 'update_disabled';
  }

  return (model.value.value.enabledPoint.value as boolean) === true ? 'update' : 'update_disabled';
});

const tempValue = computed(() => {
  if (!model?.value?.value?.valuePoint?.value) {
    return 0.0;
  }

  return (model.value.value.valuePoint.value as number).toFixed(1);
});

const spValue = computed(() => {
  if (!model?.value?.value?.setpointPoint?.value) {
    return 0.0;
  }

  return (model.value.value.setpointPoint.value as number).toFixed(1);
});

const pbValue = computed(() => {
  if (!model?.value?.value?.proportionalBandPoint) {
    return 0.0;
  }

  const sp = model.value.value.setpointPoint.value as number;
  const pb = model.value.value.proportionalBandPoint.value as number;

  return (sp + pb).toFixed(1);
});
</script>

<style lang="css" scoped>
.dashboard-cell.dashboard-temperature-controller-cell.state-offline {
  outline: 1px solid var(--clr-state-offline);
  color: var(--clr-state-offline);
}

.dashboard-cell.dashboard-temperature-controller-cell.state-disabled {
  outline: 1px solid var(--clr-state-off);
  color: var(--clr-state-off);
}

.dashboard-cell.dashboard-temperature-controller-cell.heat-off {
  outline: 1px solid var(--clr-heat-off);
  color: var(--clr-heat-off);
}

.dashboard-cell.dashboard-temperature-controller-cell.heat-on {
  outline: 1px solid var(--clr-heat-on);
  color: var(--clr-heat-on);
}

div.icon,
div.values {
  display: flex;
  flex-direction: column;
  align-content: center;
  justify-content: center;
  display: flex;
  flex-direction: column;
}

div.icon {
  width: 35%;
}

div.values {
  width: 65%;
}

div.icon > span {
  font-size: 3rem;
  display: flex;
  align-self: center;
}

div.values > div {
  display: flex;
  height: 33%;
  align-items: center;
  align-content: center;
  justify-content: center;
}

div.label {
  display: flex;
  flex-direction: row;
  line-height: 1rem;
}

div.label > span {
  font-size: 1.1rem;
  width: 25%;
}

div.label > p {
  width: 75%;
}

.values-main > span {
  font-size: 2rem;
}

.values-sub > span {
  font-size: 1rem;
}
</style>
