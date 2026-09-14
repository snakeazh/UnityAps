using System;
using System.Text;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    public sealed class RawFileHandle : CustomYieldInstruction
    {
        Action<RawFileHandle> _completed;
        int _refCount = 1;

        public bool IsDone { get; private set; }
        public bool LastOperationSucceed { get; private set; } = true;
        public string Location { get; }
        public string Error { get; private set; } = string.Empty;
        public float Progress { get; private set; }
        public byte[] RawData { get; private set; }
        public string FilePath { get; private set; }
        public override bool keepWaiting => !IsDone;

        public event Action<RawFileHandle> Completed
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

        internal RawFileHandle(string location)
        {
            Location = location ?? string.Empty;
        }

        internal void Complete(byte[] data, string path, bool success, string error = null)
        {
            if (IsDone)
            {
                return;
            }

            RawData = data;
            FilePath = path;
            LastOperationSucceed = success && data != null;
            Error = LastOperationSucceed ? string.Empty : (error ?? "Raw load failed");
            Progress = 1f;
            IsDone = true;
            _completed?.Invoke(this);
        }

        public byte[] GetRawFileData() => RawData;

        public string GetRawFileText() =>
            RawData == null ? null : Encoding.UTF8.GetString(RawData);

        public string GetRawFilePath() => FilePath;

        public void Retain() => _refCount++;

        public void Release()
        {
            _refCount--;
            if (_refCount <= 0)
            {
                RawData = null;
            }
        }

        public Flow ToFlow()
        {
            if (IsDone)
            {
                return LastOperationSucceed
                    ? Flow.Completed()
                    : Flow.FromException(new InvalidOperationException(Error));
            }

            return Flow.Create(flow =>
            {
                Completed += _ =>
                {
                    if (LastOperationSucceed)
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
