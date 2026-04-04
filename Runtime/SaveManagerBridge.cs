#if ANIMATIONMANAGER_SM
using UnityEngine;
using SaveManager.Runtime;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge between AnimationManager and SaveManager.
    /// Enable define <c>ANIMATIONMANAGER_SM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Persists the active animation id in the save slot so it can be restored on load.
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/Save Manager Bridge")]
    [DisallowMultipleComponent]
    public class SaveManagerBridge : MonoBehaviour
    {
        private const string SaveKey = "anim.currentId";

        private AnimationManager _anim;
        private SaveManager.Runtime.SaveManager _save;

        private void Awake()
        {
            _anim = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            _save = GetComponent<SaveManager.Runtime.SaveManager>()
                    ?? FindFirstObjectByType<SaveManager.Runtime.SaveManager>();

            if (_anim == null) Debug.LogWarning("[AnimationManager/SaveManagerBridge] AnimationManager not found.");
            if (_save == null) Debug.LogWarning("[AnimationManager/SaveManagerBridge] SaveManager not found.");
        }

        private void OnEnable()
        {
            if (_anim != null) _anim.OnAnimationStarted += OnAnimationStarted;
            if (_save != null) _save.OnLoaded           += OnLoaded;
        }

        private void OnDisable()
        {
            if (_anim != null) _anim.OnAnimationStarted -= OnAnimationStarted;
            if (_save != null) _save.OnLoaded           -= OnLoaded;
        }

        private void OnAnimationStarted(string id)
        {
            _save?.SetCustom(SaveKey, id);
        }

        private void OnLoaded(int slot)
        {
            if (_anim == null || _save == null) return;
            string restoredId = _save.GetCustom(SaveKey);
            if (!string.IsNullOrEmpty(restoredId) && _anim.HasAnimation(restoredId))
                _anim.Play(restoredId);
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_SM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/Save Manager Bridge")]
    public class SaveManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationManager/SaveManagerBridge] Bridge disabled — add ANIMATIONMANAGER_SM to Scripting Define Symbols.");
    }
}
#endif
