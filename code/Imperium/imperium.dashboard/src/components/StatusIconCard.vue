<template>
  <div class="spacer spacer-left">
    <span
      v-if="icon"
      :class="`material-symbols-outlined ${cssClass}`"
      :style="`color: ${color};`"
      >{{ icon }}</span
    >
  </div>
</template>

<script setup lang="ts">
import type { StateUpdate } from '@/models/dashboard';
import { computed } from 'vue';

interface Props {
  iconOff: string;
  iconOn: string;
  colorOff?: string;
  colorOn?: string;
  state: StateUpdate;
}

const props = withDefaults(defineProps<Props>(), {
  colorOff: '#838282',
  colorOn: '#01a301'
});

const icon = computed((): string => {
  return props.state() ? props.iconOn : props.iconOff;
});

const color = computed(() => {
  return props.state() ? props.colorOn : props.colorOff;
});

const cssClass = computed((): string => {
  return props.state() ? 'on' : 'off';
});
</script>

<style lang="css" scoped>
.spacer {
  min-width: 80px;
}

.spacer-left,
.spacer-right {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-content: center;
}

.spacer-left span,
.spacer-right span {
  text-align: center;
  font-size: 4rem;
}

.spacer-left span.off {
  text-align: center;
  font-size: 4rem;
}

.spacer-left span.on {
  text-align: center;
  font-size: 4rem;
}
</style>
