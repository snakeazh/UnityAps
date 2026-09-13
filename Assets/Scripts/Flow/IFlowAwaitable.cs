namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Implement or inherit <see cref="Flow"/> to become awaitable without a return value.
    /// </summary>
    public interface IFlowAwaitable
    {
        FlowAwaiter GetAwaiter();
    }

    /// <summary>
    /// Implement or inherit <see cref="Flow{T}"/> to become awaitable with a typed result.
    /// </summary>
    public interface IFlowAwaitable<out T>
    {
        FlowAwaiter<T> GetAwaiter();
    }
}
