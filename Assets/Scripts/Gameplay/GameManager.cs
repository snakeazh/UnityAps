using System;
using System.Threading;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Owns flip requests, persistent statistics, gameplay gating via <see cref="GameFlowController"/>,
    /// and a per-flip <see cref="FlowStateMachine{TState,TTrigger}"/> (Idle ⇄ Flipping).
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        const string PrefHeads = "CoinFlip.Heads";
        const string PrefTails = "CoinFlip.Tails";
        const string PrefTotal = "CoinFlip.Total";

        [SerializeField] CoinController coin;
        [SerializeField] GameFlowController flow;

        GameServices _services;
        FlowStateMachine<MatchState, MatchTrigger> _match;
        bool _matchStarted;

        public int HeadsCount { get; private set; }
        public int TailsCount { get; private set; }
        public int TotalFlips { get; private set; }
        public CoinSide? LastResult { get; private set; }
        public bool IsBusy => coin != null && coin.IsFlipping;

        public MatchState MatchPhase => _match != null ? _match.Current : MatchState.Idle;
        public GameServices Services => _services;

        /// <summary>
        /// True only after startup reaches Playing, app is not paused, and match is Idle.
        /// </summary>
        public bool CanAcceptGameplayInput =>
            (flow == null || flow.CanAcceptGameplayInput) &&
            (_services == null || !_services.IsPaused) &&
            !IsBusy &&
            (_match == null || _match.IsIn(MatchState.Idle));

        public event Action StatsChanged;
        public event Action FlipStarted;
        public event Action Landed;
        public event Action<CoinSide> FlipResolved;
        public event Action<MatchState, MatchState> MatchStateChanged;

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

        public void BindFlow(GameFlowController flowController) => flow = flowController;

        public void BindServices(GameServices services) => _services = services;

        void Awake()
        {
            LoadStats();
            EnsureMatchMachine();
        }

        void OnDestroy()
        {
            if (coin != null)
            {
                coin.FlipStarted -= OnFlipStarted;
                coin.Landed -= OnLanded;
                coin.FlipCompleted -= OnFlipCompleted;
            }

            _match?.Cancel();
            if (_match != null)
            {
                _match.StateChanged -= OnMatchStateChanged;
            }
        }

        public bool TryFlip()
        {
            if (!CanAcceptGameplayInput || coin == null)
            {
                return false;
            }

            EnsureMatchMachine();
            if (!_match.CanFire(MatchTrigger.Flip))
            {
                return false;
            }

            _match.FireAsync(MatchTrigger.Flip).Forget();
            return true;
        }

        public void ResetStats()
        {
            if (!CanAcceptGameplayInput)
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

        void EnsureMatchMachine()
        {
            if (_match != null)
            {
                return;
            }

            _match = FlowStateMachine.Create<MatchState, MatchTrigger>()
                .Initial(MatchState.Idle)
                .OnBusy(FlowStateBusyBehavior.Ignore)
                .OnError(FlowStateErrorPolicy<MatchState>.GoTo(MatchState.Idle))
                .State(MatchState.Idle, _ => { })
                .State(MatchState.Flipping, s => s
                    .OnEnter(EnterFlipping)
                    .AutoAdvanceTo(MatchState.Idle))
                .Permit(MatchState.Idle, MatchTrigger.Flip, MatchState.Flipping)
                .Build();
            _match.StateChanged += OnMatchStateChanged;

            if (!_matchStarted)
            {
                _matchStarted = true;
                _match.StartAsync().Forget();
            }
        }

        Flow EnterFlipping(FlowStateContext<MatchState, MatchTrigger> ctx)
        {
            if (coin == null || !coin.TryFlip())
            {
                return Flow.FromException(new InvalidOperationException("Coin flip could not start."));
            }

            return WaitFlipCompleted(ctx.CancellationToken);
        }

        Flow WaitFlipCompleted(CancellationToken cancellationToken)
        {
            var wait = FlowPool.RentVoid();
            wait.AttachCancellation(cancellationToken);
            if (wait.IsCompleted)
            {
                return wait;
            }

            if (coin == null)
            {
                wait.TrySetException(new InvalidOperationException("Coin missing."));
                return wait;
            }

            void Handler(CoinSide _)
            {
                coin.FlipCompleted -= Handler;
                if (!wait.IsCompleted)
                {
                    wait.TrySetResult();
                }
            }

            coin.FlipCompleted += Handler;
            cancellationToken.Register(() => coin.FlipCompleted -= Handler);

            if (!coin.IsFlipping && !wait.IsCompleted)
            {
                coin.FlipCompleted -= Handler;
                wait.TrySetResult();
            }

            return wait;
        }

        void OnMatchStateChanged(MatchState from, MatchState to) => MatchStateChanged?.Invoke(from, to);

        void OnFlipStarted() => FlipStarted?.Invoke();

        void OnLanded() => Landed?.Invoke();

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
