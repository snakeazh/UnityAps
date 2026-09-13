using System;
using System.Threading;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Optional MonoBehaviour host: owns lifetime cancellation and can auto-start a machine.
    /// Prefer wiring the machine yourself when you need custom construction (see GameFlowController).
    /// </summary>
    public abstract class FlowStateMachineHost<TState, TTrigger> : MonoBehaviour
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        [SerializeField] bool startOnStart = true;

        CancellationTokenSource _lifetimeCts;
        Flow _runFlow;

        protected FlowStateMachine<TState, TTrigger> Machine { get; private set; }

        public TState Current => Machine != null ? Machine.Current : default;
        public bool IsBusy => Machine != null && Machine.IsBusy;

        public event Action<TState, TState> StateChanged;
        public event Action<Exception> Faulted;

        protected virtual void Awake()
        {
            _lifetimeCts = new CancellationTokenSource();
            Machine = BuildMachine();
            Machine.StateChanged += OnMachineStateChanged;
            Machine.Faulted += OnMachineFaulted;
        }

        protected virtual void Start()
        {
            if (startOnStart)
            {
                Run();
            }
        }

        protected virtual void OnDestroy()
        {
            try
            {
                _lifetimeCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            Machine?.Cancel();
            _runFlow?.TrySetCanceled();
            if (Machine != null)
            {
                Machine.StateChanged -= OnMachineStateChanged;
                Machine.Faulted -= OnMachineFaulted;
            }

            _lifetimeCts?.Dispose();
        }

        /// <summary>Build the machine (called once from Awake).</summary>
        protected abstract FlowStateMachine<TState, TTrigger> BuildMachine();

        /// <summary>Start the machine (idempotent if already started — will throw from StartAsync).</summary>
        public void Run()
        {
            if (Machine == null || Machine.IsStarted)
            {
                return;
            }

            _runFlow = Machine.StartAsync(_lifetimeCts.Token);
            _runFlow.Forget();
        }

        public Flow FireAsync(TTrigger trigger) =>
            Machine.FireAsync(trigger, _lifetimeCts.Token);

        public Flow TryFireAsync(TTrigger trigger) =>
            Machine.TryFireAsync(trigger, _lifetimeCts.Token);

        public Flow GotoAsync(TState state) =>
            Machine.GotoAsync(state, _lifetimeCts.Token);

        void OnMachineStateChanged(TState from, TState to) => StateChanged?.Invoke(from, to);

        void OnMachineFaulted(Exception ex) => Faulted?.Invoke(ex);
    }
}
