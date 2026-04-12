#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using AnimationManager.Runtime;
using UnityEditor;
using UnityEngine;

namespace AnimationManager.Editor
{
    // ────────────────────────────────────────────────────────────────────────────
    // Animation JSON Editor Window
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Editor window for creating and editing <c>animations.json</c> in StreamingAssets.
    /// Open via <b>JSON Editors → Animation Manager</b> or via the Manager Inspector button.
    /// </summary>
    public class AnimationJsonEditorWindow : EditorWindow
    {
        private const string JsonFolderName   = "animations";
        private const string JsonSaveFileName = "animations.json";

        private AnimationEditorBridge    _bridge;
        private UnityEditor.Editor       _bridgeEditor;
        private Vector2                  _scroll;
        private string                   _status;
        private bool                     _statusError;

        [MenuItem("JSON Editors/Animation Manager")]
        public static void ShowWindow() =>
            GetWindow<AnimationJsonEditorWindow>("Animation JSON");

        private void OnEnable()
        {
            _bridge = CreateInstance<AnimationEditorBridge>();
            Load();
        }

        private void OnDisable()
        {
            if (_bridgeEditor != null) DestroyImmediate(_bridgeEditor);
            if (_bridge      != null) DestroyImmediate(_bridge);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, _statusError ? MessageType.Error : MessageType.Info);

            if (_bridge == null) return;
            if (_bridgeEditor == null)
                _bridgeEditor = UnityEditor.Editor.CreateEditor(_bridge);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _bridgeEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField(
                $"StreamingAssets/{JsonFolderName}/",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            string folderPath = Path.Combine(Application.streamingAssetsPath, JsonFolderName);
            try
            {
                var list = new List<AnimationDefinition>();
                if (Directory.Exists(folderPath))
                {
                    foreach (var file in Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        var w = JsonUtility.FromJson<AnimationEditorWrapper>(File.ReadAllText(file));
                        if (w?.animations != null) list.AddRange(w.animations);
                    }
                }
                else
                {
                    Directory.CreateDirectory(folderPath);
                    File.WriteAllText(Path.Combine(folderPath, JsonSaveFileName), JsonUtility.ToJson(new AnimationEditorWrapper(), true));
                    AssetDatabase.Refresh();
                }
                _bridge.animations = list;
                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }
                _status = $"Loaded {list.Count} animations from {JsonFolderName}/.";
                _statusError = false;
            }
            catch (Exception e) { _status = $"Load error: {e.Message}"; _statusError = true; }
        }

        private void Save()
        {
            try
            {
                string folderPath = Path.Combine(Application.streamingAssetsPath, JsonFolderName);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var w = new AnimationEditorWrapper { animations = _bridge.animations.ToArray() };
                var path = Path.Combine(folderPath, JsonSaveFileName);
                File.WriteAllText(path, JsonUtility.ToJson(w, true));
                AssetDatabase.Refresh();
                _status = $"Saved {_bridge.animations.Count} animations to {JsonFolderName}/{JsonSaveFileName}.";
                _statusError = false;
            }
            catch (Exception e) { _status = $"Save error: {e.Message}"; _statusError = true; }
        }
    }

    // ── ScriptableObject bridge ──────────────────────────────────────────────
    internal class AnimationEditorBridge : ScriptableObject
    {
        public List<AnimationDefinition> animations = new List<AnimationDefinition>();
    }

    // ── Local wrapper mirrors the internal AnimationManifestJson ─────────────
    [Serializable]
    internal class AnimationEditorWrapper
    {
        public AnimationDefinition[] animations = Array.Empty<AnimationDefinition>();
    }
}
#endif
