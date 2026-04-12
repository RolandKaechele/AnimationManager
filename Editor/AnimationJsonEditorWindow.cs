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
        private const string JsonFileName = "animations.json";

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
                Path.Combine("StreamingAssets", JsonFileName),
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
            try
            {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, JsonUtility.ToJson(new AnimationEditorWrapper(), true));
                    AssetDatabase.Refresh();
                }

                var w = JsonUtility.FromJson<AnimationEditorWrapper>(File.ReadAllText(path));
                _bridge.animations = new List<AnimationDefinition>(
                    w.animations ?? Array.Empty<AnimationDefinition>());

                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }

                _status     = $"Loaded {_bridge.animations.Count} animation definitions.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Load error: {e.Message}";
                _statusError = true;
            }
        }

        private void Save()
        {
            try
            {
                var w    = new AnimationEditorWrapper { animations = _bridge.animations.ToArray() };
                var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
                File.WriteAllText(path, JsonUtility.ToJson(w, true));
                AssetDatabase.Refresh();
                _status     = $"Saved {_bridge.animations.Count} animations to {JsonFileName}.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Save error: {e.Message}";
                _statusError = true;
            }
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
