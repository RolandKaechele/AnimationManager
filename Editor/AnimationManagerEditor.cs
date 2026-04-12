using UnityEditor;
using UnityEngine;
using AnimationManager.Runtime;

namespace AnimationManager.Editor
{
    [CustomEditor(typeof(AnimationManager.Runtime.AnimationManager))]
    public class AnimationManagerEditor : UnityEditor.Editor
    {
        private string _previewId = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Open JSON Editor")) AnimationJsonEditorWindow.ShowWindow();

            var manager = (AnimationManager.Runtime.AnimationManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Controls", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use live controls.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Current Animation", manager.CurrentAnimationId ?? "(none)");
            EditorGUILayout.LabelField("Is Playing", manager.IsPlaying.ToString());

            EditorGUILayout.Space();
            _previewId = EditorGUILayout.TextField("Animation Id", _previewId);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play"))         manager.Play(_previewId);
            if (GUILayout.Button("CrossFade"))    manager.CrossFade(_previewId);
            if (GUILayout.Button("Stop"))         manager.Stop();
            EditorGUILayout.EndHorizontal();

            if (manager.Animations.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Registered Animations", EditorStyles.boldLabel);
                foreach (var kvp in manager.Animations)
                    EditorGUILayout.LabelField($"  {kvp.Key}", kvp.Value.displayName ?? kvp.Key);
            }

            Repaint();
        }
    }
}
