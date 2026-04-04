#if ANIMATIONMANAGER_STM
using System.Collections.Generic;
using UnityEngine;
using StateManager.Runtime;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge between AnimationManager and StateManager.
    /// Enable define <c>ANIMATIONMANAGER_STM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Maps each <see cref="AppState"/> to an animation id and calls
    /// <see cref="AnimationManager.CrossFade(string, float)"/> whenever
    /// <see cref="StateManager.Runtime.StateManager.OnStateChanged"/> fires.
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/State Manager Bridge")]
    [DisallowMultipleComponent]
    public class StateManagerBridge : MonoBehaviour
    {
        [System.Serializable]
        public class StateAnimationMapping
        {
            [Tooltip("Application state.")]
            public AppState state;

            [Tooltip("Animation id to play when this state becomes active.")]
            public string animationId;
        }

        [Tooltip("Mapping of app states to animation ids.")]
        [SerializeField] private List<StateAnimationMapping> stateMappings = new List<StateAnimationMapping>();

        private AnimationManager _anim;
        private StateManager.Runtime.StateManager _state;

        private void Awake()
        {
            _anim  = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            _state = GetComponent<StateManager.Runtime.StateManager>()
                     ?? FindFirstObjectByType<StateManager.Runtime.StateManager>();

            if (_anim  == null) Debug.LogWarning("[AnimationManager/StateManagerBridge] AnimationManager not found.");
            if (_state == null) Debug.LogWarning("[AnimationManager/StateManagerBridge] StateManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null) _state.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (_state != null) _state.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            if (_anim == null) return;
            foreach (var mapping in stateMappings)
            {
                if (mapping.state == next && !string.IsNullOrEmpty(mapping.animationId))
                {
                    _anim.CrossFade(mapping.animationId);
                    return;
                }
            }
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_STM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/State Manager Bridge")]
    public class StateManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationManager/StateManagerBridge] Bridge disabled — add ANIMATIONMANAGER_STM to Scripting Define Symbols.");
    }
}
#endif
