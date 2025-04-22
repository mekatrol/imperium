<template>
  <div :class="`overlay ${fullScreen ? 'overlay-full-screen' : ''} ${show ? 'overlay-show' : ''}`">
    <div class="overlay-dialog">
      <div :class="`overlay-title ${data?.type.toLocaleLowerCase()}`">
        <FontAwesomeIcon
          :icon="iconType"
          class="fa-2x"
        />
        <p v-if="data?.title">{{ data?.title }}</p>
        <div class="title-spacer"></div>
        <button
          class="title-close"
          title="Close"
          @click="closeMessage"
        >
          <FontAwesomeIcon
            :icon="faClose"
            class="fa-2x"
          />
        </button>
      </div>
      <div class="overlay-message">
        <p>{{ data?.message }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { faClose, faCircleInfo, faCircleQuestion, faCircleCheck, faTriangleExclamation, faCircleExclamation, type IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { useAppStore } from '@/stores/app-store';
import { MessageType } from '@/services/message';
import { computed } from 'vue';
import type { MessageData } from '@/services/message';

interface Props {
  show: boolean;
  fullScreen?: boolean;
  data: MessageData | undefined;
}

const props = withDefaults(defineProps<Props>(), {
  show: false
});

const closeMessage = (): void => {
  const { closeMessageOverlay } = useAppStore();
  closeMessageOverlay();
};

const iconType = computed((): IconDefinition => {
  switch (props.data?.type) {
    case MessageType.Info:
      return faCircleInfo;

    case MessageType.Success:
      return faCircleCheck;

    case MessageType.Warn:
      return faTriangleExclamation;

    case MessageType.Error:
      return faCircleExclamation;

    default:
      return faCircleQuestion;
  }
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

  .overlay-dialog {
    position: absolute;
    left: 50%;
    top: 50%;
    width: var(--message-size, 800px);
    height: var(--message-size, 300px);
    transform: translate(-50%, -50%);
    padding: 2rem;

    display: flex;
    flex-direction: column;

    > div {
      display: flex;
    }

    .overlay-title {
      background-color: transparent;
      padding: 0.5rem;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.2em;
      font-weight: 700;

      &.info {
        background-color: var(--clr-information);
      }

      &.success {
        background-color: var(--clr-success);
      }

      &.warn {
        background-color: var(--clr-warning);
        color: #222;
      }

      &.error {
        background-color: var(--clr-error);
      }

      .title-spacer {
        margin: auto;
      }

      .title-close {
        background-color: transparent;
        border: none;
        cursor: pointer;
        color: inherit;
        padding: 0.2rem;
        border-radius: 3px;

        &:hover {
          background-color: #222;
          color: #eee;
        }
      }
    }

    .overlay-message {
      background-color: black;
      padding: 0.5rem;
      height: 100%;
    }
  }
}
</style>
