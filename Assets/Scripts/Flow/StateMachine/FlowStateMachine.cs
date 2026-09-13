using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Flow-native state machine with trigger edges and auto-advance.
    /// After a successful <c>OnEnter</c>, follows <see cref="FlowStateConfigurator{TState,TTrigger}.AutoAdvanceTo"/>
    /// until a state without auto-advance (stable).
    /// </summary>
    public sealed class FlowStateMachine<TState, TTrigger>
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        static readonly EqualityComparer<TState> StateComparer = EqualityComparer<TState>.Default;
        static readonly EqualityComparer<TTrigger> TriggerComparer = EqualityComparer<TTrigger>.Default;

        readonly TState _initial;
        readonly Dictionary<TState, FlowStateDefinition<TState, TTrigger>> _states;
        readonly List<FlowTransition<TState, TTrigger>> _transitions;
        readonly FlowStateErrorPolicy<TState> _errorPolicy;
        readonly FlowStateBusyBehavior _busyBehavior;

        CancellationTokenSource _runCts;
        int _runId;
        bool _busy;
        bool _started;

        internal FlowStateMachine(
            TState initial,
            Dictionary<TState, FlowStateDefinition<TState, TTrigger>> states,
            List<FlowTransition<TState, TTrigger>> transitions,
            FlowStateErrorPolicy<TState> errorPolicy,
            FlowStateBusyBehavior busyBehavior)
        {
            _initial = initial;
            _states = states;
            _transitions = transitions;
            _errorPolicy = errorPolicy;
            _busyBehavior = busyBehavior;
            // Stay at default(TState) until StartAsync — for GameFlowState this is None.
            Current = default;
            Previous = null;
            Blackboard = new FlowStateBlackboard();
        }

        public TState Current { get; private set; }
        public TState? Previous { get; private set; }
        public bool IsBusy => _busy;
        public bool IsStarted => _started;
        public FlowStateBlackboard Blackboard { get; }

        public event Action<TState, TState> StateChanged;
        public event Action<Exception> Faulted;

        /// <summary>Recent state visits (oldest → newest), capped.</summary>
        public IReadOnlyList<TState> History => _history;

        const int MaxHistory = 32;
        readonly List<TState> _history = new List<TState>(8);

        /// <summary>True when <paramref name="trigger"/> has a permitted edge from <see cref="Current"/>.</summary>
        public bool CanFire(TTrigger trigger) => TryResolve(Current, trigger, out _);

        /// <summary>Fire if permitted; otherwise returns a completed Flow (no-op).</summary>
        public Flow TryFireAsync(TTrigger trigger, CancellationToken cancellationToken = default)
        {
            if (!CanFire(trigger))
            {
                return Flow.Completed();
            }

            return FireAsync(trigger, cancellationToken);
        }

        /// <summary>True when the machine is in <paramref name="state"/> and not mid-transition.</summary>
        public bool IsIn(TState state) =>
            !_busy && StateComparer.Equals(Current, state);

        /// <summary>Completes when the machine reaches <paramref name="state"/> (or is already there and idle).</summary>
        public async Flow WaitUntilAsync(TState state, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsIn(state))
                {
                    return;
                }

                await Flow.NextFrame(cancellationToken);
            }

            throw new OperationCanceledException(cancellationToken);
        }

        /// <summary>Completes when the machine is not busy.</summary>
        public async Flow WaitUntilIdleAsync(CancellationToken cancellationToken = default)
        {
            while (_busy && !cancellationToken.IsCancellationRequested)
            {
                await Flow.NextFrame(cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        /// <summary>
        /// Enter the configured initial state, then auto-advance until stable.
        /// </summary>
        public async Flow StartAsync(CancellationToken cancellationToken = default)
        {
            if (_started)
            {
                throw new InvalidOperationException("State machine already started.");
            }

            _started = true;
            await RunTransitionAsync(Current, _initial, cancellationToken, skipExit: true);
        }

        /// <summary>Fire a trigger from the current state, then auto-advance if configured.</summary>
        public async Flow FireAsync(TTrigger trigger, CancellationToken cancellationToken = default)
        {
            EnsureStarted();
            if (!TryResolve(Current, trigger, out var to))
            {
                Debug.LogWarning($"[FlowStateMachine] No transition: {Current} + {trigger}.");
                return;
            }

            await RunTransitionAsync(Current, to, cancellationToken, skipExit: false);
        }

        /// <summary>Force-enter a state (runs exit/enter), then auto-advance if configured.</summary>
        public async Flow GotoAsync(TState state, CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                _started = true;
            }

            await RunTransitionAsync(Current, state, cancellationToken, skipExit: false);
        }

        public void Cancel()
        {
            try
            {
                _runCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        async Flow RunTransitionAsync(
            TState from,
            TState to,
            CancellationToken externalToken,
            bool skipExit)
        {
            if (_busy)
            {
                if (_busyBehavior == FlowStateBusyBehavior.Ignore)
                {
                    Debug.LogWarning($"[FlowStateMachine] Busy; ignoring → {to}.");
                    return;
                }

                Cancel();
            }

            var runId = ++_runId;
            _busy = true;
            ReplaceRunCts(externalToken);
            var token = _runCts.Token;

            try
            {
                await TransitionChainAsync(from, to, token, skipExit);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Faulted?.Invoke(ex);
                if (!await HandleErrorAsync(ex, token))
                {
                    throw;
                }
            }
            finally
            {
                if (runId == _runId)
                {
                    _busy = false;
                }
            }
        }

        async Flow TransitionChainAsync(
            TState from,
            TState to,
            CancellationToken token,
            bool skipExit)
        {
            var comingFrom = from;
            var next = to;
            var firstHop = true;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (!(firstHop && skipExit) &&
                    !StateComparer.Equals(comingFrom, next))
                {
                    await InvokeExitAsync(comingFrom, next, token);
                }

                firstHop = false;
                PublishState(next);

                await InvokeEnterAsync(next, comingFrom, token);

                if (!_states.TryGetValue(next, out var def) || !def.HasAutoAdvance)
                {
                    return;
                }

                if (StateComparer.Equals(next, def.AutoAdvanceTo))
                {
                    Debug.LogError(
                        $"[FlowStateMachine] AutoAdvance loop detected on {next}.");
                    return;
                }

                comingFrom = next;
                next = def.AutoAdvanceTo;
            }
        }

        async Flow InvokeEnterAsync(TState state, TState previous, CancellationToken token)
        {
            if (!_states.TryGetValue(state, out var def) || def.OnEnter == null)
            {
                return;
            }

            var ctx = new FlowStateContext<TState, TTrigger>(this, state, previous, token);
            var enter = def.OnEnter(ctx) ?? Flow.Completed();
            enter.AttachCancellation(token);
            await enter;
        }

        async Flow InvokeExitAsync(TState state, TState next, CancellationToken token)
        {
            if (!_states.TryGetValue(state, out var def) || def.OnExit == null)
            {
                return;
            }

            var ctx = new FlowStateContext<TState, TTrigger>(this, state, previous: state, token);
            var exit = def.OnExit(ctx) ?? Flow.Completed();
            exit.AttachCancellation(token);
            await exit;
        }

        async Flow<bool> HandleErrorAsync(Exception ex, CancellationToken token)
        {
            Debug.LogException(ex);

            switch (_errorPolicy.Action)
            {
                case FlowStateErrorAction.Propagate:
                    return false;

                case FlowStateErrorAction.Stay:
                    return true;

                case FlowStateErrorAction.GoToState:
                    // Avoid recursive error handling loops by staying busy-unlocked path:
                    // enter fallback without treating it as a nested RunTransition error bubble.
                    try
                    {
                        await TransitionChainAsync(
                            Current,
                            _errorPolicy.FallbackState,
                            token,
                            skipExit: false);
                        return true;
                    }
                    catch (Exception nested)
                    {
                        Debug.LogException(nested);
                        return false;
                    }

                default:
                    return false;
            }
        }

        void PublishState(TState next)
        {
            if (StateComparer.Equals(Current, next))
            {
                return;
            }

            Previous = Current;
            Current = next;
            RecordHistory(next);
            StateChanged?.Invoke(Previous.Value, Current);
            Debug.Log($"[FlowStateMachine] {Previous} → {Current}");
        }

        void RecordHistory(TState state)
        {
            _history.Add(state);
            if (_history.Count > MaxHistory)
            {
                _history.RemoveAt(0);
            }
        }

        bool TryResolve(TState from, TTrigger trigger, out TState to)
        {
            var ctx = new FlowStateContext<TState, TTrigger>(
                this, from, Previous, _runCts?.Token ?? CancellationToken.None);

            for (var i = 0; i < _transitions.Count; i++)
            {
                var edge = _transitions[i];
                if (!StateComparer.Equals(edge.From, from) ||
                    !TriggerComparer.Equals(edge.Trigger, trigger))
                {
                    continue;
                }

                if (edge.Guard != null && !edge.Guard(ctx))
                {
                    continue;
                }

                to = edge.To;
                return true;
            }

            to = default;
            return false;
        }

        void EnsureStarted()
        {
            if (!_started)
            {
                throw new InvalidOperationException("Call StartAsync before FireAsync.");
            }
        }

        void ReplaceRunCts(CancellationToken externalToken)
        {
            try
            {
                _runCts?.Cancel();
                _runCts?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }

            _runCts = externalToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(externalToken)
                : new CancellationTokenSource();
        }
    }
}
