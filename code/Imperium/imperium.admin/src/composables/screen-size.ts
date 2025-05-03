import { ref, onMounted, onUnmounted, type Ref } from 'vue';

export interface ScreenSize {
  width: number;
  height: number;
}

export const useScreenSize = (): Ref<ScreenSize> => {
  const screenSize = ref({ width: 0, height: 0 } as ScreenSize);

  function update(): void {
    screenSize.value = { width: window.innerWidth, height: window.innerHeight };
  }

  onMounted(() => window.addEventListener('resize', update));
  onUnmounted(() => window.removeEventListener('resize', update));

  // Force update on create
  update();

  return screenSize;
};
