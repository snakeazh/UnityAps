using CoinFlip.Assets;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip
{
    /// <summary>
    /// Empty Boot scene entry: initialize the YooAsset-aligned package, then load Main by location.
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public sealed class BootSceneLoader : MonoBehaviour
    {
        [SerializeField] string packageName = GameAssetLocations.DefaultPackage;
        [SerializeField] string targetSceneName = GameAssetLocations.SceneMain;
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
            var init = GameAssets.EnsureInitializedAsync(packageName);
            await init;

            if (minHoldSeconds > 0f)
            {
                await Flow.Delay(minHoldSeconds);
            }

            var handle = GameAssets.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
            await handle;
        }
    }
}
