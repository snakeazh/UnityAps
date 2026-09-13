namespace CoinFlip
{
    /// <summary>
    /// High-level startup / runtime phases owned by <see cref="GameFlowController"/>.
    /// </summary>
    public enum GameFlowState
    {
        None = 0,
        /// <summary>Apply runtime settings and build the playable world.</summary>
        Booting = 1,
        /// <summary>Diary cover splash; input does not flip the coin yet.</summary>
        Splash = 2,
        /// <summary>Transition out of splash into gameplay.</summary>
        Entering = 3,
        /// <summary>Player may flip the coin.</summary>
        Playing = 4
    }
}
