#if UNITY_EDITOR
using System;
using System.IO;
using CoinFlip.Assets;
using UnityEditor;
using UnityEngine;

namespace CoinFlip.EditorTools
{
    /// <summary>Independent resource version publish panel.</summary>
    public sealed class ResourceVersionPublishWindow : EditorWindow
    {
        const string PrefBump = "CoinFlip.ResourceVersion.Bump";
        const string PrefAutoBump = "CoinFlip.ResourceVersion.AutoBump";
        const string PrefCopyStreaming = "CoinFlip.ResourceVersion.CopyStreaming";
        const string PrefExportRoot = "CoinFlip.ResourceVersion.ExportRoot";
        const string PrefCustomVersion = "CoinFlip.ResourceVersion.CustomVersion";

        ResourceSettings _settings;
        EVersionBump _bump = EVersionBump.Patch;
        bool _autoBump = true;
        bool _copyToStreaming = true;
        bool _alsoRebuildBundles;
        string _exportRoot = "Publish/ResourceVersions";
        string _customVersion = string.Empty;
        Vector2 _scroll;
        string _status = string.Empty;

        [MenuItem("CoinFlip/Resource Version Publish Panel", priority = 25)]
        public static void Open()
        {
            var window = GetWindow<ResourceVersionPublishWindow>("Resource Version");
            window.minSize = new Vector2(420, 460);
            window.Show();
        }

        void OnEnable()
        {
            _settings = ResourceEditorMenu.LoadOrCreateSettings();
            _bump = (EVersionBump)EditorPrefs.GetInt(PrefBump, (int)EVersionBump.Patch);
            _autoBump = EditorPrefs.GetBool(PrefAutoBump, true);
            _copyToStreaming = EditorPrefs.GetBool(PrefCopyStreaming, true);
            _exportRoot = EditorPrefs.GetString(PrefExportRoot, "Publish/ResourceVersions");
            _customVersion = EditorPrefs.GetString(PrefCustomVersion, string.Empty);
        }

        void OnGUI()
        {
            if (_settings == null)
            {
                _settings = ResourceEditorMenu.LoadOrCreateSettings();
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("资源版本发布", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "可只刷新 version.json / bootstrap 并导出发布包，或连同 AssetBundle 一起构建。",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Resource Settings", _settings, typeof(ResourceSettings), false);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("当前版本", _settings.packageVersion);
            var preview = string.IsNullOrWhiteSpace(_customVersion)
                ? VersionBumpUtility.Preview(_settings.packageVersion, _autoBump ? _bump : EVersionBump.None)
                : _customVersion.Trim();
            EditorGUILayout.LabelField("发布后版本", preview);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("版本号", EditorStyles.boldLabel);
            _autoBump = EditorGUILayout.ToggleLeft("自动 bump 版本号", _autoBump);
            using (new EditorGUI.DisabledScope(!_autoBump || !string.IsNullOrWhiteSpace(_customVersion)))
            {
                _bump = (EVersionBump)EditorGUILayout.EnumPopup("Bump 类型", _bump);
            }

            _customVersion = EditorGUILayout.TextField("自定义版本（优先）", _customVersion);
            if (GUILayout.Button("清空自定义，使用自动 bump"))
            {
                _customVersion = string.Empty;
                GUI.FocusControl(null);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("发布选项", EditorStyles.boldLabel);
            _copyToStreaming = EditorGUILayout.ToggleLeft("拷贝 version/bootstrap 到 StreamingAssets", _copyToStreaming);
            _alsoRebuildBundles = EditorGUILayout.ToggleLeft("同时完整重建 AssetBundles", _alsoRebuildBundles);
            _exportRoot = EditorGUILayout.TextField("导出版本目录", _exportRoot);
            EditorGUILayout.LabelField(
                "目标平台",
                EditorUserBuildSettings.activeBuildTarget.ToString());

            EditorGUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("只构建/发布资源版本", GUILayout.Height(32)))
                {
                    Publish(versionOnly: true);
                }

                if (GUILayout.Button("完整构建 AB + 发布版本", GUILayout.Height(32)))
                {
                    Publish(versionOnly: false);
                }
            }

            if (GUILayout.Button("仅 bump 并保存 packageVersion（不写文件）"))
            {
                ApplyVersionBump(saveOnly: true);
            }

            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(_status, MessageType.None);
            }

            EditorGUILayout.EndScrollView();
            PersistPrefs();
        }

        void PersistPrefs()
        {
            EditorPrefs.SetInt(PrefBump, (int)_bump);
            EditorPrefs.SetBool(PrefAutoBump, _autoBump);
            EditorPrefs.SetBool(PrefCopyStreaming, _copyToStreaming);
            EditorPrefs.SetString(PrefExportRoot, _exportRoot ?? string.Empty);
            EditorPrefs.SetString(PrefCustomVersion, _customVersion ?? string.Empty);
        }

        void ApplyVersionBump(bool saveOnly)
        {
            var next = !string.IsNullOrWhiteSpace(_customVersion)
                ? _customVersion.Trim()
                : (_autoBump
                    ? VersionBumpUtility.Bump(_settings.packageVersion, _bump)
                    : _settings.packageVersion);

            _settings.packageVersion = next;
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            _status = saveOnly
                ? $"已保存 packageVersion = {next}"
                : $"版本已更新为 {next}";
            Repaint();
        }

        void Publish(bool versionOnly)
        {
            try
            {
                ApplyVersionBump(saveOnly: false);
                var target = EditorUserBuildSettings.activeBuildTarget;
                var pipeline = new AssetBundleBuildPipeline(_settings);

                if (versionOnly && !_alsoRebuildBundles)
                {
                    pipeline.PublishVersionOnly(target, _copyToStreaming, _exportRoot);
                    _status =
                        $"已发布资源版本 {_settings.packageVersion}（仅 version/bootstrap）\n" +
                        $"输出: {_settings.bundleOutputRoot}/{target}\n" +
                        $"导出: {_exportRoot}";
                }
                else
                {
                    pipeline
                        .CollectAndAssign()
                        .WriteCatalog()
                        .WriteFirstPackageManifest()
                        .BuildBundles(target)
                        .CopyFirstPackageToStreaming(target);

                    if (!string.IsNullOrWhiteSpace(_exportRoot))
                    {
                        var output = Path.Combine(_settings.bundleOutputRoot, target.ToString());
                        AssetBundleBuildPipeline.ExportVersionPackage(output, _exportRoot.Trim());
                    }

                    _status =
                        $"已完整构建并发布 {_settings.packageVersion}\n" +
                        $"输出: {_settings.bundleOutputRoot}/{target}\n" +
                        $"导出: {_exportRoot}";
                }

                Debug.Log("[ResourceVersion] " + _status.Replace('\n', ' '));
            }
            catch (Exception ex)
            {
                _status = "发布失败: " + ex.Message;
                Debug.LogException(ex);
            }

            Repaint();
        }
    }
}
#endif
