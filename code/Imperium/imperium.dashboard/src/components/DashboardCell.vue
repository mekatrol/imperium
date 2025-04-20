<template>
  <div :class="`dashboard-cell${cssClass ? ' ' + cssClass : ''} ${getCssClass()} `">
    <button @click="toggleValue" :disabled="isOffline()">
      <span v-if="icon" class="material-symbols-outlined">{{ icon }}</span>
      <p>{{ label }}</p>
    </button>
  </div>
</template>

<script setup lang="ts">
import { PointType, type Point } from '@/models/point';
import { showErrorMessage } from '@/services/message';
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
    let value: number | boolean = 0;
    if (model.value.value.pointType === PointType.Boolean) {
      value = model.value.value.value === true ? false : true;
    } else {
      value = model.value.value.value === 1 ? 0 : 1;
    }

    await appStore.updatePoint(model.value.value.id, value);
  } catch {
  }
};

</script>
