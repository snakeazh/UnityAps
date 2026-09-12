#if UNITY_EDITOR
using System.IO;
using CoinFlip;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoinFlip.EditorTools
{
    /// <summary>
    /// One-click scene / Android build helpers for this project.
    /// </summary>
    public static class CoinFlipEditorMenu
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("CoinFlip/Setup Main Scene", priority = 0)]
        public static void SetupMainScene()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 1.1f, -4.2f);
                cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
                cam.backgroundColor = new Color(0.07f, 0.09f, 0.12f);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Main scene created at " + ScenePath);
        }

        [MenuItem("CoinFlip/Configure Android Player Settings", priority = 1)]
        public static void ConfigureAndroid()
        {
            PlayerSettings.companyName = "UnityAps";
            PlayerSettings.productName = "CoinFlip";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.unityaps.coinflip");
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            Debug.Log("Android player settings applied. Active build target: Android.");
        }

        [MenuItem("CoinFlip/Build Android APK (Development)", priority = 2)]
        public static void BuildAndroidApk()
        {
            ConfigureAndroid();
            if (!File.Exists(ScenePath))
            {
                SetupMainScene();
            }

            Directory.CreateDirectory("Builds/Android");
            var apkPath = "Builds/Android/CoinFlip.apk";
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"Android build result: {report.summary.result} → {apkPath}");
        }
    }
}
#endif
