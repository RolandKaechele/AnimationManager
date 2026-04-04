#if ANIMATIONMANAGER_CSM
using UnityEngine;
using CutsceneManager.Runtime;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge between AnimationManager and CutsceneManager.
    /// Enable define <c>ANIMATIONMANAGER_CSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Listens for <see cref="CutsceneManager.Runtime.CutsceneManager.OnCustomEvent"/> and
    /// interprets the following payload prefixes:
    /// <list type="bullet">
    ///   <item><c>"anim.play:&lt;id&gt;"</c>  — calls <see cref="AnimationManager.Play(string)"/></item>
    ///   <item><c>"anim.fade:&lt;id&gt;"</c>  — calls <see cref="AnimationManager.CrossFade(string)"/> with the default duration</item>
    ///   <item><c>"anim.stop"</c>             — calls <see cref="AnimationManager.Stop"/></item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/Cutscene Manager Bridge")]
    [DisallowMultipleComponent]
    public class CutsceneManagerBridge : MonoBehaviour
    {
        private AnimationManager _anim;
        private CutsceneManager.Runtime.CutsceneManager _csm;

        private void Awake()
        {
            _anim = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            _csm  = GetComponent<CutsceneManager.Runtime.CutsceneManager>()
                    ?? FindFirstObjectByType<CutsceneManager.Runtime.CutsceneManager>();

            if (_anim == null) Debug.LogWarning("[AnimationManager/CutsceneManagerBridge] AnimationManager not found.");
            if (_csm  == null) Debug.LogWarning("[AnimationManager/CutsceneManagerBridge] CutsceneManager not found.");
        }

        private void OnEnable()
        {
            if (_csm != null) _csm.OnCustomEvent += OnCustomEvent;
        }

        private void OnDisable()
        {
            if (_csm != null) _csm.OnCustomEvent -= OnCustomEvent;
        }

        private void OnCustomEvent(string sequenceId, string eventData)
        {
            if (_anim == null || string.IsNullOrEmpty(eventData)) return;

            if (eventData.StartsWith("anim.play:", System.StringComparison.OrdinalIgnoreCase))
            {
                string id = eventData.Substring("anim.play:".Length).Trim();
                _anim.Play(id);
            }
            else if (eventData.StartsWith("anim.fade:", System.StringComparison.OrdinalIgnoreCase))
            {
                string id = eventData.Substring("anim.fade:".Length).Trim();
                _anim.CrossFade(id);
            }
            else if (eventData.Equals("anim.stop", System.StringComparison.OrdinalIgnoreCase))
            {
                _anim.Stop();
            }
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_CSM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/Cutscene Manager Bridge")]
    public class CutsceneManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationManager/CutsceneManagerBridge] Bridge disabled — add ANIMATIONMANAGER_CSM to Scripting Define Symbols.");
    }
}
#endif
