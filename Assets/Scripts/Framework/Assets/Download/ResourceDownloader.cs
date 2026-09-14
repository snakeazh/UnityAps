using System;
using System.Collections;
using System.Collections.Generic;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinFlip.Assets
{
    /// <summary>Command: download bundles by name/tag with progress + retry.</summary>
    public sealed class ResourceDownloader : AsyncOperationBase
    {
        readonly IBundleFileSystem _fs;
        readonly string _remoteRoot;
        readonly List<VersionBundleInfo> _pending;
        readonly int _maxRetry;
        readonly float _timeout;
        int _index;
        string _attemptError;

        public ResourceDownloader(
            IBundleFileSystem fs,
            string remoteRoot,
            List<VersionBundleInfo> pending,
            int maxRetry,
            float timeoutSeconds)
        {
            _fs = fs;
            _remoteRoot = (remoteRoot ?? string.Empty).TrimEnd('/');
            _pending = pending ?? new List<VersionBundleInfo>();
            _maxRetry = Math.Max(0, maxRetry);
            _timeout = timeoutSeconds;
            TotalBytes = 0;
            for (var i = 0; i < _pending.Count; i++)
            {
                TotalBytes += Math.Max(0, _pending[i].size);
            }
        }

        public void Begin()
        {
            if (_pending.Count == 0)
            {
                CompleteSuccess();
                return;
            }

            FlowRunner.StartRoutine(Run());
        }

        IEnumerator Run()
        {
            long doneBytes = 0;
            for (_index = 0; _index < _pending.Count; _index++)
            {
                if (IsCanceled)
                {
                    yield break;
                }

                var info = _pending[_index];
                _attemptError = null;
                var ok = false;
                var attempt = 0;
                while (!ok)
                {
                    if (IsCanceled)
                    {
                        yield break;
                    }

                    var url = _remoteRoot + "/" + info.name;
                    using (var req = UnityWebRequest.Get(url))
                    {
                        req.timeout = Mathf.CeilToInt(_timeout);
                        var op = req.SendWebRequest();
                        while (!op.isDone)
                        {
                            var fileProg = req.downloadProgress;
                            var overall = TotalBytes > 0
                                ? (doneBytes + info.size * fileProg) / TotalBytes
                                : (_index + fileProg) / _pending.Count;
                            SetProgress(overall);
                            BytesDownloaded = doneBytes + (long)(info.size * fileProg);
                            yield return null;
                        }

#if UNITY_2020_2_OR_NEWER
                        if (req.result != UnityWebRequest.Result.Success)
#else
                        if (req.isNetworkError || req.isHttpError)
#endif
                        {
                            _attemptError = req.error;
                        }
                        else
                        {
                            var data = req.downloadHandler.data;
                            var writeOk = false;
                            string writeErr = null;
                            yield return _fs.WriteCacheAsync(info.name, data, (s, e) =>
                            {
                                writeOk = s;
                                writeErr = e;
                            });
                            if (!writeOk)
                            {
                                _attemptError = writeErr ?? "Write cache failed";
                            }
                            else
                            {
                                ok = true;
                                doneBytes += info.size > 0 ? info.size : data.LongLength;
                                BytesDownloaded = doneBytes;
                            }
                        }
                    }

                    if (ok)
                    {
                        break;
                    }

                    attempt++;
                    RetryCount = attempt;
                    if (attempt > _maxRetry)
                    {
                        CompleteFail($"Download '{info.name}' failed: {_attemptError}");
                        yield break;
                    }

                    yield return new WaitForSecondsRealtime(Mathf.Min(8f, 0.5f * Mathf.Pow(2f, attempt - 1)));
                }
            }

            SetProgress(1f);
            CompleteSuccess();
        }
    }
}
