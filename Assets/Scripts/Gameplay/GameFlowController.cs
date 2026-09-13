using System;
using System.Threading;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Startup controller built on <see cref="FlowStateMachine{TState,TTrigger}"/>.
    /// Pipeline (auto-advance): Booting → Splash → Entering → Playing.
    /// On enter fault: Failed → (auto) Playing (fail-open).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] float splashMinSeconds = 1.5f;
        [SerializeField] float enterFadeSeconds = 0.4f;
        [SerializeField] bool allowTapToSkipSplash = true;

        GameBootstrap _bootstrap;
        SplashView _splash;
        GameManager _gameManager;
        FlowStateMachine<GameFlowState, GameFlowTrigger> _machine;
        Flow _startupFlow;
        CancellationTokenSource _lifetimeCts;
        bool _startupCompletedRaised;

        public GameFlowState State => _machine != null ? _machine.Current : GameFlowState.None;
        public bool IsPlaying => State == GameFlowState.Playing;
        public bool CanAcceptGameplayInput => IsPlaying;

        public GameManager GameManager => _gameManager;

        public event Action<GameFlowState, GameFlowState> StateChanged;
        public event Action StartupCompleted;
        public event Action<Exception> StartupFailed;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = false;
            _lifetimeCts = new CancellationTokenSource();
            _machine = BuildMachine();
            _machine.StateChanged += HandleStateChanged;
            _machine.Faulted += HandleFaulted;
        }

        void Start()
        {
            _startupFlow = _machine.StartAsync(_lifetimeCts.Token);
            _startupFlow.Forget();
        }

        void OnDestroy()
        {
            try
            {
                _lifetimeCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _machine?.Cancel();
            _startupFlow?.TrySetCanceled();
            _lifetimeCts?.Dispose();
            if (_machine != null)
            {
                _machine.StateChanged -= HandleStateChanged;
                _machine.Faulted -= HandleFaulted;
            }
        }

        FlowStateMachine<GameFlowState, GameFlowTrigger> BuildMachine()
        {
            return FlowStateMachine.Create<GameFlowState, GameFlowTrigger>()
                .Initial(GameFlowState.Booting)
                .OnBusy(FlowStateBusyBehavior.Ignore)
                .OnError(FlowStateErrorPolicy<GameFlowState>.GoTo(GameFlowState.Failed))
                .State(GameFlowState.Booting, s => s
                    .OnEnter(EnterBooting)
                    .AutoAdvanceTo(GameFlowState.Splash))
                .State(GameFlowState.Splash, s => s
                    .OnEnter(EnterSplash)
                    .AutoAdvanceTo(GameFlowState.Entering))
                .State(GameFlowState.Entering, s => s
                    .OnEnter(EnterEntering)
                    .AutoAdvanceTo(GameFlowState.Playing))
                .State(GameFlowState.Playing, s => s
                    .OnEnter(_ => Flow.Completed()))
                .State(GameFlowState.Failed, s => s
                    .OnEnter(_ => Flow.Completed())
                    .AutoAdvanceTo(GameFlowState.Playing))
                .Permit(GameFlowState.Failed, GameFlowTrigger.Recover, GameFlowState.Playing)
                .PermitAny(GameFlowTrigger.ForcePlay, GameFlowState.Playing)
                .Build();
        }

        Flow EnterBooting(FlowStateContext<GameFlowState, GameFlowTrigger> ctx)
        {
            _bootstrap = GetComponent<GameBootstrap>() ?? gameObject.AddComponent<GameBootstrap>();
            var context = _bootstrap.Build();
            _gameManager = context.Manager;
            _splash = context.Splash;
            _gameManager.BindFlow(this);
            return Flow.Completed();
        }

        Flow EnterSplash(FlowStateContext<GameFlowState, GameFlowTrigger> ctx)
        {
            if (_splash != null)
            {
                return new SplashCoverFlow(_splash, splashMinSeconds, allowTapToSkipSplash);
            }

            return Flow.Delay(splashMinSeconds, cancellationToken: ctx.CancellationToken);
        }

        Flow EnterEntering(FlowStateContext<GameFlowState, GameFlowTrigger> ctx)
        {
            if (_splash != null)
            {
                return Flow.FromCoroutine(
                    _splash.Hide(enterFadeSeconds),
                    ctx.CancellationToken);
            }

            return Flow.NextFrame(ctx.CancellationToken);
        }

        void HandleStateChanged(GameFlowState from, GameFlowState to)
        {
            StateChanged?.Invoke(from, to);
            if (to == GameFlowState.Playing)
            {
                RaiseStartupCompletedOnce();
            }
        }

        void HandleFaulted(Exception ex)
        {
            StartupFailed?.Invoke(ex);
        }

        void RaiseStartupCompletedOnce()
        {
            if (_startupCompletedRaised)
            {
                return;
            }

            _startupCompletedRaised = true;
            StartupCompleted?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Debug / Force Playing")]
        void DebugForcePlaying()
        {
            if (_splash != null)
            {
                _splash.gameObject.SetActive(false);
            }

            _machine.FireAsync(GameFlowTrigger.ForcePlay).Forget();
        }
#endif
    }

    /// <summary>
    /// Objects produced by <see cref="GameBootstrap.Build"/> for the flow controller.
    /// </summary>
    public sealed class GameBuildContext
    {
        public GameManager Manager;
        public GameUI Ui;
        public CoinController Coin;
        public CoinSparkBurst Sparks;
        public SplashView Splash;
        public Canvas RootCanvas;
    }
}
