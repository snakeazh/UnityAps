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
        const string BootScenePath = "Assets/Scenes/Boot.unity";
        const string MainScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("CoinFlip/Setup Boot Scene", priority = 0)]
        public static void SetupBootScene()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.97f, 0.93f, 0.84f);
            cam.orthographic = true;
            camGo.AddComponent<AudioListener>();

            var loaderGo = new GameObject("BootSceneLoader");
            var loader = loaderGo.AddComponent<BootSceneLoader>();
            var so = new SerializedObject(loader);
            so.FindProperty("packageName").stringValue = "DefaultPackage";
            so.FindProperty("targetSceneName").stringValue = "Main";
            so.FindProperty("minHoldSeconds").floatValue = 0.05f;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Boot scene created at " + BootScenePath + " (loads Main asynchronously).");
        }

        [MenuItem("CoinFlip/Setup Main Scene", priority = 1)]
        public static void SetupMainScene()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 0.85f, -4.6f);
                cam.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
                cam.backgroundColor = new Color(0.97f, 0.93f, 0.84f);
            }

            EditorSceneManager.SaveScene(scene, MainScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Main scene created at " + MainScenePath);
        }

        [MenuItem("CoinFlip/Configure Android Player Settings", priority = 2)]
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

        [MenuItem("CoinFlip/Build Android APK (Development)", priority = 3)]
        public static void BuildAndroidApk()
        {
            ConfigureAndroid();
            if (!File.Exists(BootScenePath))
            {
                SetupBootScene();
            }

            if (!File.Exists(MainScenePath))
            {
                SetupMainScene();
            }

            EnsureBuildSettings();
            Directory.CreateDirectory("Builds/Android");
            var apkPath = "Builds/Android/CoinFlip.apk";
            var options = new BuildPlayerOptions
            {
                scenes = new[] { BootScenePath, MainScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"Android build result: {report.summary.result} → {apkPath}");
        }

        static void EnsureBuildSettings()
        {
            var bootGuid = AssetDatabase.AssetPathToGUID(BootScenePath);
            var mainGuid = AssetDatabase.AssetPathToGUID(MainScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, !string.IsNullOrEmpty(bootGuid) || File.Exists(BootScenePath)),
                new EditorBuildSettingsScene(MainScenePath, !string.IsNullOrEmpty(mainGuid) || File.Exists(MainScenePath))
            };
        }
    }
}
#endif
