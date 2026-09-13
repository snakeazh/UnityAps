using System.Collections;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip
{
    /// <summary>
    /// Lives in the empty Boot scene. Finds the target scene in Build Settings and
    /// asynchronously replaces this scene with it (LoadSceneMode.Single).
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public sealed class BootSceneLoader : MonoBehaviour
    {
        [SerializeField] string targetSceneName = "Main";
        [SerializeField] float minHoldSeconds = 0.05f;

        Flow _loadFlow;

        void Start()
        {
            _loadFlow = TransitionAsync();
            _loadFlow.Forget();
        }

        void OnDestroy()
        {
            _loadFlow?.TrySetCanceled();
        }

        async Flow TransitionAsync()
        {
            if (!TryFindBuiltScene(targetSceneName, out var resolvedName))
            {
                Debug.LogError(
                    $"[BootSceneLoader] Scene '{targetSceneName}' was not found in Build Settings. " +
                    "Add Boot + Main via CoinFlip/Setup Boot Scene.");
                return;
            }

            if (minHoldSeconds > 0f)
            {
                await Flow.Delay(minHoldSeconds);
            }

            await Flow.FromCoroutine(LoadSingleRoutine(resolvedName));
        }

        static IEnumerator LoadSingleRoutine(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[BootSceneLoader] LoadSceneAsync failed for '{sceneName}'.");
                yield break;
            }

            while (!op.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Resolves a scene by build name (case-insensitive). Prefers an exact match.
        /// </summary>
        public static bool TryFindBuiltScene(string sceneName, out string resolvedName)
        {
            resolvedName = null;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            var count = SceneManager.sceneCountInBuildSettings;
            string fallback = null;
            for (var i = 0; i < count; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, sceneName, System.StringComparison.Ordinal))
                {
                    resolvedName = name;
                    return true;
                }

                if (fallback == null &&
                    string.Equals(name, sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    fallback = name;
                }
            }

            if (fallback != null)
            {
                resolvedName = fallback;
                return true;
            }

            // Last resort: Unity may still stream it if registered under that name.
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                resolvedName = sceneName;
                return true;
            }

            return false;
        }
    }
}
