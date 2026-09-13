namespace CoinFlip
{
    /// <summary>Triggers for <see cref="GameFlowController"/>'s Flow state machine.</summary>
    public enum GameFlowTrigger
    {
        /// <summary>Unused in auto-advance boot; reserved for manual jumps.</summary>
        None = 0,
        /// <summary>Recover from <see cref="GameFlowState.Failed"/> into Playing.</summary>
        Recover = 1,
        /// <summary>Force gameplay (debug / fail-open).</summary>
        ForcePlay = 2,
    }
}
