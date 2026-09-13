using CoinFlip.Assets;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Composition root for app-level services (not a DI container).
    /// Ensured during Booting and shared via <see cref="GameBuildContext"/>.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public sealed class GameServices : MonoBehaviour
    {
        [SerializeField] GameTuning tuningAsset;

        GameTuning _tuning;
        GameSettings _settings;
        AudioService _audio;
        AppLifecycle _lifecycle;

        public GameTuning Tuning => _tuning;
        public GameSettings Settings => _settings;
        public AudioService Audio => _audio;
        public AppLifecycle Lifecycle => _lifecycle;
        public ResourcePackage Assets => GameAssets.DefaultPackage;

        public bool IsPaused => _lifecycle != null && _lifecycle.IsPaused;
        public bool IsMuted => _settings != null && _settings.Muted;

        public static GameServices Ensure(GameObject host, GameTuning preferredTuning = null)
        {
            var services = host.GetComponent<GameServices>() ?? host.AddComponent<GameServices>();
            services.Initialize(preferredTuning);
            return services;
        }

        public void Initialize(GameTuning preferredTuning = null)
        {
            if (_settings != null)
            {
                if (preferredTuning != null)
                {
                    _tuning = preferredTuning;
                    _audio?.SetTuning(_tuning);
                }

                return;
            }

            if (!GameAssets.Initialized)
            {
                GameAssets.EnsureInitializedAsync();
            }

            _tuning = GameTuning.ResolveOrDefault(preferredTuning != null ? preferredTuning : tuningAsset);
            _settings = new GameSettings();
            _settings.Load();

            _audio = GetComponent<AudioService>() ?? gameObject.AddComponent<AudioService>();
            _audio.Initialize(_settings, _tuning);

            _lifecycle = GetComponent<AppLifecycle>() ?? gameObject.AddComponent<AppLifecycle>();
        }
    }
}
