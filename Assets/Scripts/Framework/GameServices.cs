using CoinFlip.Assets;
using CoinFlip.FlowFramework;
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
            if (services._settings == null)
            {
                // Sync path keeps preferred/serialized/default; full package resolve via EnsureAsync.
                services.InitializeSync(preferredTuning);
            }
            else if (preferredTuning != null)
            {
                services._tuning = preferredTuning;
                services._audio?.SetTuning(services._tuning);
            }

            return services;
        }

        public static async Flow<GameServices> EnsureAsync(GameObject host, GameTuning preferredTuning = null)
        {
            var services = host.GetComponent<GameServices>() ?? host.AddComponent<GameServices>();
            await services.InitializeAsync(preferredTuning);
            return services;
        }

        void InitializeSync(GameTuning preferredTuning = null)
        {
            if (_settings != null)
            {
                return;
            }

            if (!GameAssets.Initialized)
            {
                GameAssets.EnsureInitializedAsync();
            }

            _tuning = preferredTuning != null
                ? preferredTuning
                : (tuningAsset != null ? tuningAsset : GameTuning.CreateRuntimeDefault());
            BindServices();
        }

        public async Flow InitializeAsync(GameTuning preferredTuning = null)
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
                await GameAssets.EnsureInitializedAsync();
            }

            _tuning = await GameTuning.ResolveOrDefaultAsync(
                preferredTuning != null ? preferredTuning : tuningAsset);
            BindServices();
        }

        void BindServices()
        {
            _settings = new GameSettings();
            _settings.Load();

            _audio = GetComponent<AudioService>() ?? gameObject.AddComponent<AudioService>();
            _audio.Initialize(_settings, _tuning);

            _lifecycle = GetComponent<AppLifecycle>() ?? gameObject.AddComponent<AppLifecycle>();
        }
    }
}
