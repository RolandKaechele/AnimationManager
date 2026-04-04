#if ANIMATIONMANAGER_EM
using UnityEngine;
using EventManager.Runtime;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge between AnimationManager and EventManager.
    /// Enable define <c>ANIMATIONMANAGER_EM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named <see cref="GameEvent"/>s:
    /// <list type="bullet">
    ///   <item><c>"animation.started"</c>   — <see cref="GameEvent.stringValue"/> = animation id</item>
    ///   <item><c>"animation.stopped"</c>   — <see cref="GameEvent.stringValue"/> = animation id</item>
    ///   <item><c>"animation.completed"</c> — <see cref="GameEvent.stringValue"/> = animation id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/Event Manager Bridge")]
    [DisallowMultipleComponent]
    public class EventManagerBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when an animation starts.")]
        [SerializeField] private string startedEventName   = "animation.started";

        [Tooltip("Event name fired when Stop() is called.")]
        [SerializeField] private string stoppedEventName   = "animation.stopped";

        [Tooltip("Event name fired when an animation completes naturally.")]
        [SerializeField] private string completedEventName = "animation.completed";

        private EventManager.Runtime.EventManager _events;
        private AnimationManager _anim;

        private void Awake()
        {
            _events = GetComponent<EventManager.Runtime.EventManager>()
                      ?? FindFirstObjectByType<EventManager.Runtime.EventManager>();
            _anim   = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();

            if (_events == null) Debug.LogWarning("[AnimationManager/EventManagerBridge] EventManager not found.");
            if (_anim   == null) Debug.LogWarning("[AnimationManager/EventManagerBridge] AnimationManager not found.");
        }

        private void OnEnable()
        {
            if (_anim != null)
            {
                _anim.OnAnimationStarted   += OnStarted;
                _anim.OnAnimationStopped   += OnStopped;
                _anim.OnAnimationCompleted += OnCompleted;
            }
        }

        private void OnDisable()
        {
            if (_anim != null)
            {
                _anim.OnAnimationStarted   -= OnStarted;
                _anim.OnAnimationStopped   -= OnStopped;
                _anim.OnAnimationCompleted -= OnCompleted;
            }
        }

        private void OnStarted(string id)   => _events?.Fire(new GameEvent(startedEventName,   id));
        private void OnStopped(string id)   => _events?.Fire(new GameEvent(stoppedEventName,   id));
        private void OnCompleted(string id) => _events?.Fire(new GameEvent(completedEventName, id));
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_EM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/Event Manager Bridge")]
    public class EventManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationManager/EventManagerBridge] Bridge disabled — add ANIMATIONMANAGER_EM to Scripting Define Symbols.");
    }
}
#endif
