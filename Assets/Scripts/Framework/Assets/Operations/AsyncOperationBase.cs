using System;
using System.Collections;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Template Method base for async commands (Progress / Error / Retry / Completed).</summary>
    public abstract class AsyncOperationBase : CustomYieldInstruction
    {
        Action<AsyncOperationBase> _completed;
        bool _canceled;

        public bool IsDone { get; private set; }
        public bool StatusIsSucceed { get; private set; } = true;
        public string Error { get; private set; } = string.Empty;
        public float Progress { get; protected set; }
        public long BytesDownloaded { get; protected set; }
        public long TotalBytes { get; protected set; }
        public int RetryCount { get; protected set; }
        public override bool keepWaiting => !IsDone;

        public event Action<AsyncOperationBase> Completed
        {
            add
            {
                _completed += value;
                if (IsDone && value != null)
                {
                    value(this);
                }
            }
            remove => _completed -= value;
        }

        public event Action<AsyncOperationBase> ProgressChanged;

        protected void SetProgress(float value)
        {
            Progress = Mathf.Clamp01(value);
            ProgressChanged?.Invoke(this);
        }

        protected void CompleteSuccess()
        {
            if (IsDone)
            {
                return;
            }

            StatusIsSucceed = true;
            Error = string.Empty;
            Progress = 1f;
            IsDone = true;
            _completed?.Invoke(this);
        }

        protected void CompleteFail(string error)
        {
            if (IsDone)
            {
                return;
            }

            StatusIsSucceed = false;
            Error = error ?? "Unknown error";
            IsDone = true;
            _completed?.Invoke(this);
        }

        public virtual void Cancel()
        {
            _canceled = true;
            if (!IsDone)
            {
                CompleteFail("Canceled");
            }
        }

        protected bool IsCanceled => _canceled;

        /// <summary>Template: run with retry + timeout.</summary>
        protected IEnumerator RunWithRetry(IEnumerator step, int maxRetry, float timeoutSeconds)
        {
            var attempt = 0;
            while (true)
            {
                if (_canceled)
                {
                    CompleteFail("Canceled");
                    yield break;
                }

                var failed = false;
                string failError = null;
                var elapsed = 0f;
                var e = step;
                // Re-create step each attempt via factory if needed — subclasses override CreateAttempt.
                e = CreateAttempt(attempt);
                while (e.MoveNext())
                {
                    if (_canceled)
                    {
                        CompleteFail("Canceled");
                        yield break;
                    }

                    elapsed += Time.unscaledDeltaTime;
                    if (timeoutSeconds > 0f && elapsed >= timeoutSeconds)
                    {
                        failed = true;
                        failError = $"Timeout after {timeoutSeconds}s";
                        break;
                    }

                    if (TryReadAttemptFailure(out var err))
                    {
                        failed = true;
                        failError = err;
                        break;
                    }

                    yield return e.Current;
                }

                if (!failed && !TryReadAttemptFailure(out failError))
                {
                    OnAttemptSucceeded();
                    CompleteSuccess();
                    yield break;
                }

                RetryCount = ++attempt;
                if (attempt > maxRetry)
                {
                    CompleteFail(failError ?? "Failed after retries");
                    yield break;
                }

                var backoff = Mathf.Min(8f, 0.5f * Mathf.Pow(2f, attempt - 1));
                yield return new WaitForSecondsRealtime(backoff);
                ResetAttemptState();
            }
        }

        protected virtual IEnumerator CreateAttempt(int attempt) { yield break; }
        protected virtual bool TryReadAttemptFailure(out string error) { error = null; return false; }
        protected virtual void OnAttemptSucceeded() { }
        protected virtual void ResetAttemptState() { }

        public Flow ToFlow()
        {
            if (IsDone)
            {
                return StatusIsSucceed
                    ? Flow.Completed()
                    : Flow.FromException(new InvalidOperationException(Error));
            }

            return Flow.Create(flow =>
            {
                Completed += _ =>
                {
                    if (StatusIsSucceed)
                    {
                        flow.TrySetResult();
                    }
                    else
                    {
                        flow.TrySetException(new InvalidOperationException(Error));
                    }
                };
            });
        }

        public FlowAwaiter GetAwaiter() => ToFlow().GetAwaiter();
    }
}
