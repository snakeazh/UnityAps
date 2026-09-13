using System;
using System.Collections;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Example of inheriting <see cref="Flow"/> so a custom type is directly awaitable.
    /// <code>
    /// yield return new SplashCoverFlow(view, 1.5f, true).ToYieldInstruction();
    /// // or inside async Task:
    /// await new SplashCoverFlow(view, 1.5f, true);
    /// </code>
    /// </summary>
    public sealed class SplashCoverFlow : Flow
    {
        public SplashCoverFlow(global::CoinFlip.SplashView splash, float minSeconds, bool allowTapSkip)
        {
            if (splash == null)
            {
                SetResult();
                return;
            }

            FlowRunner.StartRoutine(Run(splash, minSeconds, allowTapSkip));
        }

        IEnumerator Run(global::CoinFlip.SplashView splash, float minSeconds, bool allowTapSkip)
        {
            IEnumerator play = null;
            try
            {
                play = splash.Play(minSeconds, allowTapSkip);
            }
            catch (Exception ex)
            {
                SetException(ex);
                yield break;
            }

            while (true)
            {
                bool moved;
                try
                {
                    moved = play.MoveNext();
                }
                catch (Exception ex)
                {
                    SetException(ex);
                    yield break;
                }

                if (!moved)
                {
                    break;
                }

                yield return play.Current;
            }

            SetResult();
        }
    }
}
