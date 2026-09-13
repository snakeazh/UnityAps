namespace CoinFlip
{
    /// <summary>Per-flip gameplay phases owned by <see cref="GameManager"/>.</summary>
    public enum MatchState
    {
        Idle = 0,
        Flipping = 1,
    }

    /// <summary>Triggers for the match Flow state machine.</summary>
    public enum MatchTrigger
    {
        Flip = 1,
    }
}
