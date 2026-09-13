using System;
using System.Collections;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip
{
    /// <summary>
    /// Startup flow controller: Booting → Splash → Entering → Playing.
    /// Owns the launch sequence; gameplay input stays locked until <see cref="GameFlowState.Playing"/>.
    /// Driven by the <see cref="Flow"/> promise framework (awaitable + WhenAll).
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

        public GameFlowState State { get; private set; } = GameFlowState.None;
        public bool IsPlaying => State == GameFlowState.Playing;
        public bool CanAcceptGameplayInput => IsPlaying;

        public GameManager GameManager => _gameManager;

        public event Action<GameFlowState, GameFlowState> StateChanged;
        public event Action StartupCompleted;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = false;
        }

        void Start()
        {
            StartCoroutine(RunStartup());
        }

        IEnumerator RunStartup()
        {
            SetState(GameFlowState.Booting);

            _bootstrap = GetComponent<GameBootstrap>() ?? gameObject.AddComponent<GameBootstrap>();
            var context = _bootstrap.Build();
            _gameManager = context.Manager;
            _splash = context.Splash;
            _gameManager.BindFlow(this);

            SetState(GameFlowState.Splash);
            if (_splash != null)
            {
                // Inherit Flow to make a domain step directly awaitable / yieldable.
                yield return new SplashCoverFlow(_splash, splashMinSeconds, allowTapToSkipSplash)
                    .ToYieldInstruction();
            }
            else
            {
                yield return Flow.Delay(splashMinSeconds).ToYieldInstruction();
            }

            SetState(GameFlowState.Entering);
            if (_splash != null)
            {
                yield return Flow.FromCoroutine(_splash.Hide(enterFadeSeconds)).ToYieldInstruction();
            }

            // One frame so UI layout settles before flips are allowed.
            yield return Flow.NextFrame().ToYieldInstruction();

            SetState(GameFlowState.Playing);
            StartupCompleted?.Invoke();
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
            StopAllCoroutines();
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
