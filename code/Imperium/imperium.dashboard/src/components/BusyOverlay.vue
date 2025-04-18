<template>
  <div :class="`overlay ${fullScreen ? 'overlay-full-screen' : ''} ${show ? 'overlay-show' : ''}`">
    <div class="overlay-spinner"></div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  show: boolean;
  fullScreen?: boolean;
}

withDefaults(defineProps<Props>(), {
  show: false
});
</script>

<style scoped lang="css">
/* Adapted from https://www.w3schools.com/howto/howto_css_overlay.asp */
.overlay {
  /* Fill parent (overlay siblings) */
  position: absolute;

  /* Hidden by default */
  display: none;

  /* Full width (cover the whole page) */
  width: 100%;

  /* Full height (cover the whole page) */
  height: 100%;

  /* Pin to each corner */
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;

  background-color: var(--overlay-background-color, rgb(0, 0, 0, 0.5));
  z-index: var(--overlay-z-index, 99999);

  &.overlay-full-screen {
    /* Fill page (overlay page) */
    position: fixed;
  }

  &.overlay-show {
    display: revert;
  }

  /* Adapted from https://www.w3schools.com/howto/howto_css_loader.asp */
  .overlay-spinner {
    position: absolute;
    left: 50%;
    top: 50%;
    -webkit-animation: spinner-spin 2s linear infinite; /* Safari */
    animation: spinner-spin 2s linear infinite;

    width: var(--spinner-size, 120px);
    height: var(--spinner-size, 120px);

    border: 18px solid var(--spinner-border-color, #f3f3f3);
    border-top: 18px solid var(--spinner-spinner-color, #3498db);
    border-radius: 50%;
    margin: -76px 0 0 -76px;
  }
}

@keyframes spinner-spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
