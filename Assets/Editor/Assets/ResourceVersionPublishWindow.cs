#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using CoinFlip.Assets;
using UnityEditor;
using UnityEngine;

namespace CoinFlip.EditorTools
{
    /// <summary>
    /// Independent resource version publish panel.
    /// CDN layout B: {cdnRoot}/{version}/ + latest.json; upload via attributed Uploader.
    /// </summary>
    public sealed class ResourceVersionPublishWindow : EditorWindow
    {
        const string PrefBump = "CoinFlip.ResourceVersion.Bump";
        const string PrefAutoBump = "CoinFlip.ResourceVersion.AutoBump";
        const string PrefCopyStreaming = "CoinFlip.ResourceVersion.CopyStreaming";
        const string PrefCdnRoot = "CoinFlip.ResourceVersion.CdnRoot";
        const string PrefCustomVersion = "CoinFlip.ResourceVersion.CustomVersion";
        const string PrefActivateLatest = "CoinFlip.ResourceVersion.ActivateLatest";
        const string PrefRunUpload = "CoinFlip.ResourceVersion.RunUpload";
        const string PrefUploader = "CoinFlip.ResourceVersion.UploaderType";

        ResourceSettings _settings;
        EVersionBump _bump = EVersionBump.Patch;
        bool _autoBump = true;
        bool _copyToStreaming = true;
        bool _alsoRebuildBundles;
        bool _activateLatest = true;
        bool _runUpload = true;
        string _cdnRoot = "Publish/cdn";
        string _customVersion = string.Empty;
        Vector2 _scroll;
        string _status = string.Empty;

        List<ResourceVersionUploaderRegistry.Entry> _uploaders =
            new List<ResourceVersionUploaderRegistry.Entry>();
        string[] _uploaderLabels = Array.Empty<string>();
        int _uploaderIndex;

        [MenuItem("CoinFlip/Resource Version Publish Panel", priority = 25)]
        public static void Open()
        {
            var window = GetWindow<ResourceVersionPublishWindow>("Resource Version");
            window.minSize = new Vector2(440, 560);
            window.Show();
        }

        void OnEnable()
        {
            _settings = ResourceEditorMenu.LoadOrCreateSettings();
            _bump = (EVersionBump)EditorPrefs.GetInt(PrefBump, (int)EVersionBump.Patch);
            _autoBump = EditorPrefs.GetBool(PrefAutoBump, true);
            _copyToStreaming = EditorPrefs.GetBool(PrefCopyStreaming, true);
            _cdnRoot = EditorPrefs.GetString(PrefCdnRoot, "Publish/cdn");
            _customVersion = EditorPrefs.GetString(PrefCustomVersion, string.Empty);
            _activateLatest = EditorPrefs.GetBool(PrefActivateLatest, true);
            _runUpload = EditorPrefs.GetBool(PrefRunUpload, true);
            RefreshUploaders();
        }

        void RefreshUploaders()
        {
            _uploaders = ResourceVersionUploaderRegistry.Discover();
            _uploaderLabels = new string[_uploaders.Count];
            for (var i = 0; i < _uploaders.Count; i++)
            {
                _uploaderLabels[i] = _uploaders[i].DisplayName;
            }

            var saved = EditorPrefs.GetString(PrefUploader, string.Empty);
            _uploaderIndex = 0;
            if (!string.IsNullOrEmpty(saved))
            {
                for (var i = 0; i < _uploaders.Count; i++)
                {
                    if (_uploaders[i].Type != null &&
                        string.Equals(_uploaders[i].Type.FullName, saved, StringComparison.Ordinal))
                    {
                        _uploaderIndex = i;
                        break;
                    }
                }
            }
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
                "布局 B：本地导出 Publish/cdn/{version}/ + latest.json。\n" +
                "上传通过继承 ResourceVersionUploaderBase + [ResourceVersionUploader] 特性发现；默认仅校验本地导出，由 CI sync 到 CDN。",
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
            EditorGUILayout.LabelField("CDN 导出", EditorStyles.boldLabel);
            _cdnRoot = EditorGUILayout.TextField("CDN Root（本地）", _cdnRoot);
            _activateLatest = EditorGUILayout.ToggleLeft("导出后写入/激活 latest.json", _activateLatest);
            _copyToStreaming = EditorGUILayout.ToggleLeft("拷贝 version/bootstrap 到 StreamingAssets", _copyToStreaming);
            _alsoRebuildBundles = EditorGUILayout.ToggleLeft("同时完整重建 AssetBundles", _alsoRebuildBundles);
            EditorGUILayout.LabelField("目标平台", EditorUserBuildSettings.activeBuildTarget.ToString());
            EditorGUILayout.SelectableLabel(
                $"{_cdnRoot}/latest.json\n{_cdnRoot}/{preview}/version.json\n{_cdnRoot}/{preview}/*.bundle",
                EditorStyles.helpBox,
                GUILayout.Height(54));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("上传（Uploader）", EditorStyles.boldLabel);
            if (_uploaderLabels.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "未发现 Uploader。请继承 ResourceVersionUploaderBase 并添加 [ResourceVersionUploader]。",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                _uploaderIndex = EditorGUILayout.Popup("Uploader", _uploaderIndex, _uploaderLabels);
                if (GUILayout.Button("↻", GUILayout.Width(28)))
                {
                    RefreshUploaders();
                }

                EditorGUILayout.EndHorizontal();
                _runUpload = EditorGUILayout.ToggleLeft("导出后执行选中 Uploader", _runUpload);
            }

            if (GUILayout.Button("刷新 Uploader 列表"))
            {
                RefreshUploaders();
            }

            EditorGUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("只发布资源版本", GUILayout.Height(32)))
                {
                    Publish(versionOnly: true);
                }

                if (GUILayout.Button("完整构建 AB + 发布", GUILayout.Height(32)))
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
            EditorPrefs.SetString(PrefCdnRoot, _cdnRoot ?? string.Empty);
            EditorPrefs.SetString(PrefCustomVersion, _customVersion ?? string.Empty);
            EditorPrefs.SetBool(PrefActivateLatest, _activateLatest);
            EditorPrefs.SetBool(PrefRunUpload, _runUpload);
            if (_uploaders.Count > 0 && _uploaderIndex >= 0 && _uploaderIndex < _uploaders.Count &&
                _uploaders[_uploaderIndex].Type != null)
            {
                EditorPrefs.SetString(PrefUploader, _uploaders[_uploaderIndex].Type.FullName);
            }
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
                var cdn = string.IsNullOrWhiteSpace(_cdnRoot) ? "Publish/cdn" : _cdnRoot.Trim();
                var pipeline = new AssetBundleBuildPipeline(_settings);
                string output;

                if (versionOnly && !_alsoRebuildBundles)
                {
                    // Export CDN separately so ActivateLatest is respected.
                    pipeline.PublishVersionOnly(target, _copyToStreaming, exportRoot: null);
                    output = Path.Combine(_settings.bundleOutputRoot, target.ToString()).Replace("\\", "/");
                }
                else
                {
                    pipeline
                        .CollectAndAssign()
                        .WriteCatalog()
                        .WriteFirstPackageManifest()
                        .BuildBundles(target)
                        .CopyFirstPackageToStreaming(target);
                    output = Path.Combine(_settings.bundleOutputRoot, target.ToString()).Replace("\\", "/");
                }

                var versionDir = AssetBundleBuildPipeline.ExportCdnLayout(
                    output, cdn, _settings.packageVersion, _activateLatest);

                var uploadNote = string.Empty;
                if (_runUpload && _uploaders.Count > 0)
                {
                    var entry = _uploaders[Mathf.Clamp(_uploaderIndex, 0, _uploaders.Count - 1)];
                    var ctx = new ResourceVersionUploadContext
                    {
                        Version = _settings.packageVersion,
                        LocalCdnRoot = cdn,
                        LocalVersionDir = versionDir,
                        LatestJsonPath = Path.Combine(cdn, LatestManifest.FileName).Replace("\\", "/"),
                        ActivateLatest = _activateLatest,
                        BuildTarget = target
                    };

                    if (entry.Instance.Upload(ctx, out var error))
                    {
                        uploadNote = $"\nUploader OK: {entry.DisplayName}";
                    }
                    else
                    {
                        uploadNote = $"\nUploader failed ({entry.DisplayName}): {error}";
                        Debug.LogError("[ResourceVersion] " + uploadNote.Trim());
                    }
                }

                _status =
                    $"已发布资源版本 {_settings.packageVersion}\n" +
                    $"CDN: {versionDir}\n" +
                    $"latest.json: {_activateLatest}" +
                    uploadNote;

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
