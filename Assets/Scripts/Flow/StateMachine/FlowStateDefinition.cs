using System;

namespace CoinFlip.FlowFramework
{
    sealed class FlowStateDefinition<TState, TTrigger>
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        public TState State { get; }
        public Func<FlowStateContext<TState, TTrigger>, Flow> OnEnter { get; set; }
        public Func<FlowStateContext<TState, TTrigger>, Flow> OnExit { get; set; }
        public bool HasAutoAdvance { get; set; }
        public TState AutoAdvanceTo { get; set; }

        public FlowStateDefinition(TState state)
        {
            State = state;
        }
    }

    sealed class FlowTransition<TState, TTrigger>
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        public TState From { get; }
        public TTrigger Trigger { get; }
        public TState To { get; }
        public Func<FlowStateContext<TState, TTrigger>, bool> Guard { get; }

        public FlowTransition(
            TState from,
            TTrigger trigger,
            TState to,
            Func<FlowStateContext<TState, TTrigger>, bool> guard)
        {
            From = from;
            Trigger = trigger;
            To = to;
            Guard = guard;
        }
    }
}
