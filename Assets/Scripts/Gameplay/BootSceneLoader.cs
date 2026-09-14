using CoinFlip.Assets;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip
{
    /// <summary>
    /// Empty Boot scene entry: initialize package, optional host update + first package, then load Main.
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public sealed class BootSceneLoader : MonoBehaviour
    {
        [SerializeField] string packageName = GameAssetLocations.DefaultPackage;
        [SerializeField] string targetSceneName = GameAssetLocations.SceneMain;
        [SerializeField] float minHoldSeconds = 0.05f;
        [SerializeField] bool updatePackageOnBoot = true;
        [SerializeField] bool preloadFirstPackage = true;

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

            var package = GameAssets.DefaultPackage;
            if (package != null &&
                package.PlayMode == EPlayMode.HostPlayMode &&
                updatePackageOnBoot)
            {
                await package.UpdatePackageAsync();
            }

            if (package != null && preloadFirstPackage)
            {
                await package.PreloadFirstPackageAsync();
            }

            if (minHoldSeconds > 0f)
            {
                await Flow.Delay(minHoldSeconds);
            }

            var handle = GameAssets.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
            await handle;
        }
    }
}
