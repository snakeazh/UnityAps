using CoinFlip.Assets;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Designer knobs for splash, coin toss, reward copy, and optional SFX slots.
    /// Missing clips are fine — <see cref="AudioService"/> no-ops when unset.
    /// </summary>
    [CreateAssetMenu(fileName = "GameTuning", menuName = "CoinFlip/Game Tuning", order = 0)]
    public sealed class GameTuning : ScriptableObject
    {
        [Header("Startup")]
        public float splashMinSeconds = 1.5f;
        public float enterFadeSeconds = 0.4f;
        public bool allowTapToSkipSplash = true;

        [Header("Coin toss")]
        public float anticipation = 0.12f;
        public float flightDuration = 1.05f;
        public float bounceDuration = 0.55f;
        public float settleDuration = 0.22f;
        public float flipHeight = 3.1f;
        public int minSpins = 5;
        public int maxSpins = 8;
        public float landSquash = 0.18f;
        public float bounceHeight = 0.55f;

        [Header("Reward copy (visual only)")]
        public string headsRewardText = "+¥1.88";
        public string tailsRewardText = "+¥0.88";

        [Header("SFX (optional)")]
        public AudioClip tossClip;
        public AudioClip landClip;
        public AudioClip uiClickClip;

        public static GameTuning CreateRuntimeDefault()
        {
            var tuning = CreateInstance<GameTuning>();
            tuning.name = "GameTuning (Runtime Default)";
            return tuning;
        }

        public static GameTuning ResolveOrDefault(GameTuning preferred = null)
        {
            if (preferred != null)
            {
                return preferred;
            }

            return CreateRuntimeDefault();
        }

        public static async Flow<GameTuning> ResolveOrDefaultAsync(GameTuning preferred = null)
        {
            if (preferred != null)
            {
                return preferred;
            }

            if (!GameAssets.Initialized)
            {
                return CreateRuntimeDefault();
            }

            var handle = GameAssets.LoadAssetAsync<GameTuning>(GameAssetLocations.GameTuning);
            await handle;
            var fromPackage = handle.GetAssetObject<GameTuning>();
            if (fromPackage != null)
            {
                return fromPackage;
            }

            return CreateRuntimeDefault();
        }
    }
}
