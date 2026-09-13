using System;
using System.Collections.Generic;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Fluent configuration for a single state inside <see cref="FlowStateMachineBuilder{TState,TTrigger}"/>.
    /// </summary>
    public sealed class FlowStateConfigurator<TState, TTrigger>
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        readonly FlowStateDefinition<TState, TTrigger> _definition;

        internal FlowStateConfigurator(FlowStateDefinition<TState, TTrigger> definition)
        {
            _definition = definition;
        }

        public FlowStateConfigurator<TState, TTrigger> OnEnter(
            Func<FlowStateContext<TState, TTrigger>, Flow> enter)
        {
            _definition.OnEnter = enter;
            return this;
        }

        public FlowStateConfigurator<TState, TTrigger> OnEnter(Func<Flow> enter)
        {
            _definition.OnEnter = _ => enter();
            return this;
        }

        public FlowStateConfigurator<TState, TTrigger> OnExit(
            Func<FlowStateContext<TState, TTrigger>, Flow> exit)
        {
            _definition.OnExit = exit;
            return this;
        }

        public FlowStateConfigurator<TState, TTrigger> OnExit(Func<Flow> exit)
        {
            _definition.OnExit = _ => exit();
            return this;
        }

        /// <summary>
        /// After <c>OnEnter</c> succeeds, automatically transition to <paramref name="next"/>.
        /// This is the primary “linear pipeline” mode.
        /// </summary>
        public FlowStateConfigurator<TState, TTrigger> AutoAdvanceTo(TState next)
        {
            _definition.HasAutoAdvance = true;
            _definition.AutoAdvanceTo = next;
            return this;
        }
    }

    /// <summary>
    /// Builds a <see cref="FlowStateMachine{TState,TTrigger}"/> with auto-advance and trigger edges.
    /// </summary>
    public sealed class FlowStateMachineBuilder<TState, TTrigger>
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        readonly Dictionary<TState, FlowStateDefinition<TState, TTrigger>> _states =
            new Dictionary<TState, FlowStateDefinition<TState, TTrigger>>();
        readonly List<FlowTransition<TState, TTrigger>> _transitions =
            new List<FlowTransition<TState, TTrigger>>();

        TState _initial;
        bool _initialSet;
        FlowStateErrorPolicy<TState> _errorPolicy = FlowStateErrorPolicy<TState>.Propagate();
        FlowStateBusyBehavior _busyBehavior = FlowStateBusyBehavior.Ignore;

        public FlowStateMachineBuilder<TState, TTrigger> Initial(TState state)
        {
            _initial = state;
            _initialSet = true;
            return this;
        }

        public FlowStateMachineBuilder<TState, TTrigger> State(
            TState state,
            Action<FlowStateConfigurator<TState, TTrigger>> configure)
        {
            if (!_states.TryGetValue(state, out var definition))
            {
                definition = new FlowStateDefinition<TState, TTrigger>(state);
                _states[state] = definition;
            }

            configure?.Invoke(new FlowStateConfigurator<TState, TTrigger>(definition));
            return this;
        }

        public FlowStateMachineBuilder<TState, TTrigger> Permit(
            TState from,
            TTrigger trigger,
            TState to,
            Func<FlowStateContext<TState, TTrigger>, bool> guard = null)
        {
            EnsureState(from);
            EnsureState(to);
            _transitions.Add(new FlowTransition<TState, TTrigger>(from, trigger, to, guard));
            return this;
        }

        /// <summary>Same trigger from every registered state (and any later-added states at build time).</summary>
        public FlowStateMachineBuilder<TState, TTrigger> PermitAny(
            TTrigger trigger,
            TState to,
            Func<FlowStateContext<TState, TTrigger>, bool> guard = null)
        {
            EnsureState(to);
            foreach (TState from in Enum.GetValues(typeof(TState)))
            {
                EnsureState(from);
                _transitions.Add(new FlowTransition<TState, TTrigger>(from, trigger, to, guard));
            }

            return this;
        }

        public FlowStateMachineBuilder<TState, TTrigger> OnError(FlowStateErrorPolicy<TState> policy)
        {
            _errorPolicy = policy;
            return this;
        }

        public FlowStateMachineBuilder<TState, TTrigger> OnBusy(FlowStateBusyBehavior behavior)
        {
            _busyBehavior = behavior;
            return this;
        }

        public FlowStateMachine<TState, TTrigger> Build()
        {
            if (!_initialSet)
            {
                throw new InvalidOperationException("Initial state is required. Call Initial(...).");
            }

            EnsureState(_initial);
            return new FlowStateMachine<TState, TTrigger>(
                _initial,
                _states,
                _transitions,
                _errorPolicy,
                _busyBehavior);
        }

        void EnsureState(TState state)
        {
            if (!_states.ContainsKey(state))
            {
                _states[state] = new FlowStateDefinition<TState, TTrigger>(state);
            }
        }
    }

    public static class FlowStateMachine
    {
        public static FlowStateMachineBuilder<TState, TTrigger> Create<TState, TTrigger>()
            where TState : struct, Enum
            where TTrigger : struct, Enum =>
            new FlowStateMachineBuilder<TState, TTrigger>();
    }
}
