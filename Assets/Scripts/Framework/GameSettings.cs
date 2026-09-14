using System;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Local player settings (separate from flip statistics in <see cref="GameManager"/>).
    /// </summary>
    public sealed class GameSettings
    {
        const string PrefMuted = "CoinFlip.Settings.Muted";
        const string PrefHaptics = "CoinFlip.Settings.HapticsEnabled";

        bool _muted;
        bool _hapticsEnabled;

        public bool Muted
        {
            get => _muted;
            set
            {
                if (_muted == value)
                {
                    return;
                }

                _muted = value;
                PlayerPrefs.SetInt(PrefMuted, _muted ? 1 : 0);
                PlayerPrefs.Save();
                MutedChanged?.Invoke(_muted);
            }
        }

        /// <summary>Reserved for a later haptics pass; persisted now.</summary>
        public bool HapticsEnabled
        {
            get => _hapticsEnabled;
            set
            {
                if (_hapticsEnabled == value)
                {
                    return;
                }

                _hapticsEnabled = value;
                PlayerPrefs.SetInt(PrefHaptics, _hapticsEnabled ? 1 : 0);
                PlayerPrefs.Save();
                HapticsChanged?.Invoke(_hapticsEnabled);
            }
        }

        public event Action<bool> MutedChanged;
        public event Action<bool> HapticsChanged;

        public void Load()
        {
            _muted = PlayerPrefs.GetInt(PrefMuted, 0) != 0;
            _hapticsEnabled = PlayerPrefs.GetInt(PrefHaptics, 1) != 0;
        }

        public void ToggleMuted() => Muted = !Muted;
    }
}
