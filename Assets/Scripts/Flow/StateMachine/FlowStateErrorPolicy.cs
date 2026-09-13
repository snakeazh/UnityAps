using System;

namespace CoinFlip.FlowFramework
{
    /// <summary>What to do when a state <c>OnEnter</c> faults.</summary>
    public enum FlowStateErrorAction
    {
        /// <summary>Propagate the exception out of <see cref="FlowStateMachine{TState,TTrigger}.StartAsync"/>.</summary>
        Propagate = 0,
        /// <summary>Move to a configured fallback state (then auto-advance from there if set).</summary>
        GoToState = 1,
        /// <summary>Stay in the current state; mark machine idle.</summary>
        Stay = 2,
    }

    public readonly struct FlowStateErrorPolicy<TState>
        where TState : struct, Enum
    {
        public FlowStateErrorAction Action { get; }
        public TState FallbackState { get; }

        FlowStateErrorPolicy(FlowStateErrorAction action, TState fallbackState)
        {
            Action = action;
            FallbackState = fallbackState;
        }

        public static FlowStateErrorPolicy<TState> Propagate() =>
            new FlowStateErrorPolicy<TState>(FlowStateErrorAction.Propagate, default);

        public static FlowStateErrorPolicy<TState> Stay() =>
            new FlowStateErrorPolicy<TState>(FlowStateErrorAction.Stay, default);

        public static FlowStateErrorPolicy<TState> GoTo(TState state) =>
            new FlowStateErrorPolicy<TState>(FlowStateErrorAction.GoToState, state);
    }

    /// <summary>How to treat Fire/Goto while a transition is already running.</summary>
    public enum FlowStateBusyBehavior
    {
        /// <summary>Ignore the request and log a warning.</summary>
        Ignore = 0,
        /// <summary>Cancel the current enter and start the new transition.</summary>
        Preempt = 1,
    }
}
