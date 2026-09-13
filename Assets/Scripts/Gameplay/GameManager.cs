using System;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Owns flip requests, persistent statistics, and gameplay gating via <see cref="GameFlowController"/>.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        const string PrefHeads = "CoinFlip.Heads";
        const string PrefTails = "CoinFlip.Tails";
        const string PrefTotal = "CoinFlip.Total";

        [SerializeField] CoinController coin;
        [SerializeField] GameFlowController flow;

        public int HeadsCount { get; private set; }
        public int TailsCount { get; private set; }
        public int TotalFlips { get; private set; }
        public CoinSide? LastResult { get; private set; }
        public bool IsBusy => coin != null && coin.IsFlipping;

        /// <summary>
        /// True only after startup reaches <see cref="GameFlowState.Playing"/>.
        /// </summary>
        public bool CanAcceptGameplayInput =>
            (flow == null || flow.CanAcceptGameplayInput) && !IsBusy;

        public event Action StatsChanged;
        public event Action FlipStarted;
        public event Action Landed;
        public event Action<CoinSide> FlipResolved;

        public void Bind(CoinController coinController)
        {
            if (coin != null)
            {
                coin.FlipStarted -= OnFlipStarted;
                coin.Landed -= OnLanded;
                coin.FlipCompleted -= OnFlipCompleted;
            }

            coin = coinController;
            if (coin != null)
            {
                coin.FlipStarted += OnFlipStarted;
                coin.Landed += OnLanded;
                coin.FlipCompleted += OnFlipCompleted;
            }
        }

        public void BindFlow(GameFlowController flowController)
        {
            flow = flowController;
        }

        void Awake()
        {
            LoadStats();
        }

        void OnDestroy()
        {
            if (coin != null)
            {
                coin.FlipStarted -= OnFlipStarted;
                coin.Landed -= OnLanded;
                coin.FlipCompleted -= OnFlipCompleted;
            }
        }

        public bool TryFlip()
        {
            if (!CanAcceptGameplayInput || coin == null)
            {
                return false;
            }

            return coin.TryFlip();
        }

        public void ResetStats()
        {
            if (flow != null && !flow.CanAcceptGameplayInput)
            {
                return;
            }

            HeadsCount = 0;
            TailsCount = 0;
            TotalFlips = 0;
            LastResult = null;
            SaveStats();
            StatsChanged?.Invoke();
        }

        void OnFlipStarted()
        {
            FlipStarted?.Invoke();
        }

        void OnLanded()
        {
            Landed?.Invoke();
        }

        void OnFlipCompleted(CoinSide side)
        {
            LastResult = side;
            TotalFlips++;
            if (side == CoinSide.Heads)
            {
                HeadsCount++;
            }
            else
            {
                TailsCount++;
            }

            SaveStats();
            FlipResolved?.Invoke(side);
            StatsChanged?.Invoke();
        }

        void LoadStats()
        {
            HeadsCount = PlayerPrefs.GetInt(PrefHeads, 0);
            TailsCount = PlayerPrefs.GetInt(PrefTails, 0);
            TotalFlips = PlayerPrefs.GetInt(PrefTotal, 0);
        }

        void SaveStats()
        {
            PlayerPrefs.SetInt(PrefHeads, HeadsCount);
            PlayerPrefs.SetInt(PrefTails, TailsCount);
            PlayerPrefs.SetInt(PrefTotal, TotalFlips);
            PlayerPrefs.Save();
        }
    }
}
