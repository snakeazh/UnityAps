using System;
using System.Threading;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Context passed to state enter/exit handlers.
    /// </summary>
    public sealed class FlowStateContext<TState, TTrigger>
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        readonly FlowStateMachine<TState, TTrigger> _machine;

        internal FlowStateContext(
            FlowStateMachine<TState, TTrigger> machine,
            TState state,
            TState? previous,
            CancellationToken cancellationToken)
        {
            _machine = machine;
            State = state;
            Previous = previous;
            CancellationToken = cancellationToken;
        }

        public FlowStateMachine<TState, TTrigger> Machine => _machine;
        public TState State { get; }
        public TState? Previous { get; }
        public CancellationToken CancellationToken { get; }

        /// <summary>Shared bag for passing data across states (e.g. bootstrap results).</summary>
        public FlowStateBlackboard Blackboard => _machine.Blackboard;
    }

    /// <summary>Simple string-keyed blackboard shared by one state machine instance.</summary>
    public sealed class FlowStateBlackboard
    {
        readonly System.Collections.Generic.Dictionary<string, object> _items =
            new System.Collections.Generic.Dictionary<string, object>();

        public void Set<T>(string key, T value) => _items[key] = value;

        public bool TryGet<T>(string key, out T value)
        {
            if (_items.TryGetValue(key, out var boxed) && boxed is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        public T Get<T>(string key) =>
            TryGet<T>(key, out var value)
                ? value
                : throw new InvalidOperationException($"Blackboard missing key '{key}'.");

        public void Clear() => _items.Clear();
    }
}
