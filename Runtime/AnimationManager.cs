using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Centralized animation controller for Unity.
    /// Manages Animator state transitions across a registered target <see cref="Animator"/>,
    /// supports JSON-driven animation definitions for modding, and exposes events
    /// for cross-module integration via bridge components.
    /// </summary>
    [AddComponentMenu("Managers/Animation Manager")]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public class AnimationManager : SerializedMonoBehaviour
#else
    public class AnimationManager : MonoBehaviour
#endif
    {
        // ──────────────────────────────────────────────────────────
        // Inspector fields
        // ──────────────────────────────────────────────────────────

        [Header("Target")]
        [Tooltip("The Animator component that will receive state transitions. Can be set at runtime via SetAnimator().")]
        [SerializeField] private Animator targetAnimator;

        [Header("Definitions")]
        [Tooltip("Built-in animation definitions. JSON entries are merged on top by id.")]
        [SerializeField] private List<AnimationDefinition> animations = new List<AnimationDefinition>();

        [Header("JSON / Modding")]
        [Tooltip("Load additional definitions from StreamingAssets/<jsonPath>.")]
        [SerializeField] private bool loadFromJson;

        [Tooltip("Path relative to StreamingAssets/.")]
        [SerializeField] private string jsonPath = "animations.json";

        [Header("Transitions")]
        [Tooltip("Default crossfade duration in seconds.")]
        [SerializeField] private float defaultCrossFadeDuration = 0.25f;

        [Header("Debug")]
        [Tooltip("Log all state transitions to the Console.")]
        [SerializeField] private bool verboseLogging;

        // ──────────────────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────────────────

        /// <summary>Fired when an animation begins playing. Parameter is the animation id.</summary>
        public event Action<string> OnAnimationStarted;

        /// <summary>Fired when <see cref="Stop"/> is called explicitly. Parameter is the animation id that was stopped.</summary>
        public event Action<string> OnAnimationStopped;

        /// <summary>Fired when the current animation finishes its natural playback (non-loop only). Parameter is the animation id.</summary>
        public event Action<string> OnAnimationCompleted;

        // ──────────────────────────────────────────────────────────
        // State
        // ──────────────────────────────────────────────────────────

        private readonly Dictionary<string, AnimationDefinition> _map = new Dictionary<string, AnimationDefinition>(StringComparer.OrdinalIgnoreCase);
        private string _currentId;
        private bool _isPlaying;
        private float _playbackTimer;
        private AnimationDefinition _currentDef;

        // ──────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────

        /// <summary>Currently playing animation id, or <c>null</c> if idle.</summary>
        public string CurrentAnimationId => _currentId;

        /// <summary>Whether an animation is currently playing.</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>All registered animation definitions, keyed by id.</summary>
        public IReadOnlyDictionary<string, AnimationDefinition> Animations => _map;

        /// <summary>
        /// Replaces the animation target at runtime.
        /// </summary>
        public void SetAnimator(Animator animator)
        {
            targetAnimator = animator;
            if (verboseLogging)
                Debug.Log($"[AnimationManager] Animator target replaced: {(animator != null ? animator.name : "null")}");
        }

        /// <summary>
        /// Plays the animation with the given <paramref name="id"/> immediately (no blend).
        /// </summary>
        public void Play(string id)
        {
            if (!TryGetDefinition(id, out var def)) return;
            ApplyAnimation(def, 0f);
        }

        /// <summary>
        /// Crossfades to the animation with the given <paramref name="id"/> using the supplied duration (or the global default).
        /// </summary>
        public void CrossFade(string id, float duration = -1f)
        {
            if (!TryGetDefinition(id, out var def)) return;
            float d = duration < 0f ? defaultCrossFadeDuration : duration;
            ApplyAnimation(def, d);
        }

        /// <summary>
        /// Stops the currently playing animation and raises <see cref="OnAnimationStopped"/>.
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying) return;
            string stopped = _currentId;
            ClearPlaybackState();

            if (verboseLogging)
                Debug.Log($"[AnimationManager] Stopped: {stopped}");

            OnAnimationStopped?.Invoke(stopped ?? string.Empty);
        }

        /// <summary>
        /// Returns <c>true</c> when a definition with the given <paramref name="id"/> exists.
        /// </summary>
        public bool HasAnimation(string id) => !string.IsNullOrEmpty(id) && _map.ContainsKey(id);

        // ──────────────────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildMap();
            if (loadFromJson) LoadJson();
        }

        private void Update()
        {
            if (!_isPlaying || _currentDef == null || _currentDef.loop) return;

            _playbackTimer -= Time.deltaTime;
            if (_playbackTimer <= 0f)
            {
                string completed = _currentId;
                ClearPlaybackState();
                if (verboseLogging)
                    Debug.Log($"[AnimationManager] Completed: {completed}");
                OnAnimationCompleted?.Invoke(completed ?? string.Empty);
            }
        }

        // ──────────────────────────────────────────────────────────
        // Internal helpers
        // ──────────────────────────────────────────────────────────

        private void BuildMap()
        {
            _map.Clear();
            foreach (var def in animations)
            {
                if (string.IsNullOrEmpty(def.id)) continue;
                _map[def.id] = def;
            }
        }

        private void LoadJson()
        {
            string full = Path.Combine(Application.streamingAssetsPath, jsonPath);
            if (!File.Exists(full))
            {
                Debug.LogWarning($"[AnimationManager] JSON not found: {full}");
                return;
            }
            try
            {
                string json = File.ReadAllText(full);
                var manifest = JsonUtility.FromJson<AnimationManifestJson>(json);
                foreach (var def in manifest.animations)
                {
                    if (string.IsNullOrEmpty(def.id)) continue;
                    _map[def.id] = def;
                }
                if (verboseLogging)
                    Debug.Log($"[AnimationManager] Loaded {manifest.animations.Count} definitions from {jsonPath}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AnimationManager] Failed to parse {jsonPath}: {ex.Message}");
            }
        }

        private bool TryGetDefinition(string id, out AnimationDefinition def)
        {
            if (string.IsNullOrEmpty(id) || !_map.TryGetValue(id, out def))
            {
                Debug.LogWarning($"[AnimationManager] Animation not found: '{id}'");
                def = null;
                return false;
            }
            return true;
        }

        private void ApplyAnimation(AnimationDefinition def, float crossFadeDuration)
        {
            if (targetAnimator == null)
            {
                Debug.LogWarning("[AnimationManager] No target Animator assigned.");
                return;
            }

            if (!string.IsNullOrEmpty(def.controllerPath))
            {
                var ctrl = Resources.Load<RuntimeAnimatorController>(def.controllerPath);
                if (ctrl != null) targetAnimator.runtimeAnimatorController = ctrl;
            }

            if (crossFadeDuration > 0f)
                targetAnimator.CrossFadeInFixedTime(def.stateName, crossFadeDuration);
            else
                targetAnimator.Play(def.stateName);

            _currentId = def.id;
            _currentDef = def;
            _isPlaying = true;
            _playbackTimer = def.duration;

            if (verboseLogging)
                Debug.Log($"[AnimationManager] Playing: {def.id} (crossFade={crossFadeDuration:F2}s)");

            OnAnimationStarted?.Invoke(def.id);
        }

        private void ClearPlaybackState()
        {
            _currentId = null;
            _currentDef = null;
            _isPlaying = false;
            _playbackTimer = 0f;
        }
    }
}
