using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Startup flow controller: Booting → Splash → Entering → Playing.
    /// Each phase is a <see cref="Flow"/> step with unified error handling.
    /// Gameplay input stays locked until <see cref="GameFlowState.Playing"/>.
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
        Flow _startupFlow;

        public GameFlowState State { get; private set; } = GameFlowState.None;
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
        }

        void Start()
        {
            _startupFlow = RunStartupAsync();
            _startupFlow.Forget();
        }

        void OnDestroy()
        {
            _startupFlow?.TrySetCanceled();
        }

        /// <summary>
        /// Entire boot pipeline as one awaitable Flow — every state step is Flow-backed
        /// so faults surface through a single handler.
        /// </summary>
        async Flow RunStartupAsync()
        {
            try
            {
                await RunBootingAsync();
                await RunSplashAsync();
                await RunEnteringAsync();
                await Flow.NextFrame();
                SetState(GameFlowState.Playing);
                StartupCompleted?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Tear-down / domain reload.
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                StartupFailed?.Invoke(ex);
                SetState(GameFlowState.Failed);
                // Fail-open so the player can still flip after a splash fault.
                SetState(GameFlowState.Playing);
                StartupCompleted?.Invoke();
            }
        }

        async Flow RunBootingAsync()
        {
            SetState(GameFlowState.Booting);
            _bootstrap = GetComponent<GameBootstrap>() ?? gameObject.AddComponent<GameBootstrap>();
            var context = _bootstrap.Build();
            _gameManager = context.Manager;
            _splash = context.Splash;
            _gameManager.BindFlow(this);
            await Flow.Completed();
        }

        async Flow RunSplashAsync()
        {
            SetState(GameFlowState.Splash);
            if (_splash != null)
            {
                await new SplashCoverFlow(_splash, splashMinSeconds, allowTapToSkipSplash);
            }
            else
            {
                await Flow.Delay(splashMinSeconds);
            }
        }

        async Flow RunEnteringAsync()
        {
            SetState(GameFlowState.Entering);
            if (_splash != null)
            {
                await Flow.FromCoroutine(_splash.Hide(enterFadeSeconds));
            }
        }

        void SetState(GameFlowState next)
        {
            if (State == next)
            {
                return;
            }

            var previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
            Debug.Log($"[GameFlow] {previous} → {next}");
        }

#if UNITY_EDITOR
        [ContextMenu("Debug / Force Playing")]
        void DebugForcePlaying()
        {
            _startupFlow?.TrySetCanceled();
            if (_splash != null)
            {
                _splash.gameObject.SetActive(false);
            }

            SetState(GameFlowState.Playing);
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
