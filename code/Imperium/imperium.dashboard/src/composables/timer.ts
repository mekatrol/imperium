import { onMounted, onUnmounted } from 'vue';

export interface IntervalTimerCallback {
  (): Promise<boolean>;
}

export const useIntervalTimer = (timerTickCallback: IntervalTimerCallback, intervalMs: number): void => {
  // Keep track of interval timer handle
  let timerHandle = undefined as number | undefined;

  const stopTimer = (): void => {
    if (timerHandle !== undefined) {
      // Stop timer
      clearInterval(timerHandle);

      // Clear handle
      timerHandle = undefined;
    }
  };

  // We need to keep track if we are in callback in case previous callback
  // still executing
  let inTickCallback = false;

  // Start timer when mounted
  onMounted(async () => {
    timerHandle = window.setInterval(async (): Promise<void> => {
      try {
        if (!inTickCallback) {
          // Entering callback method
          inTickCallback = true;

          // Await the callback task
          const continueTimer = await timerTickCallback();

          // If the callback returns false then stop timer
          if (!continueTimer) {
            stopTimer();
          }

          // No longer in callback method
          inTickCallback = false;
        }

        // Ignore any errors
      } catch {
        inTickCallback = false;
      }
    }, intervalMs);
  });

  // Stop timer when unmounted
  onUnmounted(() => {
    stopTimer();
  });
};
