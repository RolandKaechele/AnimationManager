#if ANIMATIONMANAGER_CM
using UnityEngine;
using CharacterManager.Runtime;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge between AnimationManager and CharacterManager.
    /// Enable define <c>ANIMATIONMANAGER_CM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// When <see cref="CharacterManager.Runtime.CharacterManager.OnActiveCharacterChanged"/> fires,
    /// this bridge attempts to load a <see cref="RuntimeAnimatorController"/> from
    /// <c>Resources/Characters/&lt;characterId&gt;/Animator</c> and assign it to the target Animator.
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/Character Manager Bridge")]
    [DisallowMultipleComponent]
    public class CharacterManagerBridge : MonoBehaviour
    {
        [Tooltip("Resource folder pattern used to locate the RuntimeAnimatorController for each character. " +
                 "Use {id} as a placeholder for the character id.")]
        [SerializeField] private string controllerResourcePattern = "Characters/{id}/Animator";

        private AnimationManager _anim;
        private CharacterManager.Runtime.CharacterManager _cm;

        private void Awake()
        {
            _anim = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            _cm   = GetComponent<CharacterManager.Runtime.CharacterManager>()
                    ?? FindFirstObjectByType<CharacterManager.Runtime.CharacterManager>();

            if (_anim == null) Debug.LogWarning("[AnimationManager/CharacterManagerBridge] AnimationManager not found.");
            if (_cm   == null) Debug.LogWarning("[AnimationManager/CharacterManagerBridge] CharacterManager not found.");
        }

        private void OnEnable()
        {
            if (_cm != null) _cm.OnActiveCharacterChanged += OnActiveCharacterChanged;
        }

        private void OnDisable()
        {
            if (_cm != null) _cm.OnActiveCharacterChanged -= OnActiveCharacterChanged;
        }

        private void OnActiveCharacterChanged(string characterId)
        {
            if (_anim == null || string.IsNullOrEmpty(characterId)) return;

            string path = controllerResourcePattern.Replace("{id}", characterId);
            var ctrl = Resources.Load<RuntimeAnimatorController>(path);
            if (ctrl != null)
            {
                // Find the Animator on the AnimationManager's target via reflection-safe accessor
                var animatorField = typeof(AnimationManager).GetField("targetAnimator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (animatorField?.GetValue(_anim) is Animator animator)
                    animator.runtimeAnimatorController = ctrl;
            }
            else
            {
                Debug.LogWarning($"[AnimationManager/CharacterManagerBridge] No animator controller found at Resources/{path}");
            }
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_CM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/Character Manager Bridge")]
    public class CharacterManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationManager/CharacterManagerBridge] Bridge disabled — add ANIMATIONMANAGER_CM to Scripting Define Symbols.");
    }
}
#endif
