using System;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Tracks app pause / focus. Locks new gameplay input without cancelling an in-flight flip.
    /// Does not change timeScale so an airborne coin can finish settling.
    /// </summary>
    public sealed class AppLifecycle : MonoBehaviour
    {
        bool _paused;

        public bool IsPaused => _paused;

        public event Action<bool> PauseChanged;

        public void SetPaused(bool paused)
        {
            if (_paused == paused)
            {
                return;
            }

            _paused = paused;
            PauseChanged?.Invoke(_paused);
            Debug.Log(paused ? "[AppLifecycle] Paused (input locked)." : "[AppLifecycle] Resumed.");
        }

        void OnApplicationPause(bool pauseStatus) => SetPaused(pauseStatus);

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SetPaused(true);
            }
            else
            {
                SetPaused(false);
            }
        }
    }
}
