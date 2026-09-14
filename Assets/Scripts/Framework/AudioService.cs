using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Lightweight SFX player. Respects <see cref="GameSettings.Muted"/>; null clips no-op.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        GameSettings _settings;
        GameTuning _tuning;
        AudioSource _sfx;

        public void Initialize(GameSettings settings, GameTuning tuning)
        {
            _settings = settings;
            _tuning = tuning;
            EnsureSource();
        }

        public void SetTuning(GameTuning tuning) => _tuning = tuning;

        public void PlayToss() => Play(_tuning != null ? _tuning.tossClip : null);
        public void PlayLand() => Play(_tuning != null ? _tuning.landClip : null);
        public void PlayUiClick() => Play(_tuning != null ? _tuning.uiClickClip : null);

        void Play(AudioClip clip)
        {
            if (clip == null || _settings == null || _settings.Muted)
            {
                return;
            }

            EnsureSource();
            _sfx.PlayOneShot(clip);
        }

        void EnsureSource()
        {
            if (_sfx != null)
            {
                return;
            }

            _sfx = gameObject.GetComponent<AudioSource>();
            if (_sfx == null)
            {
                _sfx = gameObject.AddComponent<AudioSource>();
            }

            _sfx.playOnAwake = false;
            _sfx.loop = false;
            _sfx.spatialBlend = 0f;
        }
    }
}
