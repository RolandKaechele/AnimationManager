#if ANIMATIONMANAGER_DOTWEEN
using UnityEngine;
using DG.Tweening;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge that adds DOTween-driven supplemental effects to animations
    /// managed by <see cref="AnimationManager"/>: a punch-scale impact on start and
    /// a DOVirtual float tween to blend the Animator parameter weight for extra polish.
    /// Enable define <c>ANIMATIONMANAGER_DOTWEEN</c> in Player Settings › Scripting Define Symbols.
    /// Requires <b>DOTween Pro</b>.
    /// </summary>
    [AddComponentMenu("AnimationManager/DOTween Bridge")]
    [DisallowMultipleComponent]
    public class DotweenAnimationBridge : MonoBehaviour
    {
        [Header("Punch Scale on Start")]
        [Tooltip("When true, the animator's transform plays a punch-scale on each animation start.")]
        [SerializeField] private bool usePunchScale = true;

        [Tooltip("Punch vector applied to the animator root on animation start.")]
        [SerializeField] private Vector3 punchScale = new Vector3(0.08f, 0.08f, 0f);

        [Tooltip("Duration of the punch-scale animation.")]
        [SerializeField] private float punchDuration = 0.25f;

        [Tooltip("Vibrato count for the punch-scale.")]
        [SerializeField] private int punchVibrato = 5;

        [Tooltip("Elasticity for the punch-scale (0 = rigid, 1 = elastic).")]
        [Range(0f, 1f)]
        [SerializeField] private float punchElasticity = 0.3f;

        [Header("Animator Float Blend")]
        [Tooltip("Name of an Animator float parameter to blend from 0→1 on start and 1→0 on complete. " +
                 "Leave empty to disable float blending.")]
        [SerializeField] private string blendParamName = "";

        [Tooltip("Duration for the blend-to-1 tween on animation start.")]
        [SerializeField] private float blendInDuration = 0.15f;

        [Tooltip("Duration for the blend-to-0 tween on animation complete.")]
        [SerializeField] private float blendOutDuration = 0.2f;

        [Tooltip("DOTween ease for blend transitions.")]
        [SerializeField] private Ease blendEase = Ease.InOutQuad;

        // -------------------------------------------------------------------------

        private AnimationManager _anim;
        private Animator         _animator;
        private Tween            _blendTween;

        private void Awake()
        {
            _anim = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            if (_anim == null)
            {
                Debug.LogWarning("[AnimationManager/DotweenAnimationBridge] AnimationManager not found.");
                return;
            }

            // Mirror the same target Animator used by AnimationManager (accessed via reflection).
            var field = typeof(AnimationManager).GetField("targetAnimator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _animator = field?.GetValue(_anim) as Animator;
        }

        private void OnEnable()
        {
            if (_anim == null) return;
            _anim.OnAnimationStarted   += OnAnimationStarted;
            _anim.OnAnimationCompleted += OnAnimationCompleted;
            _anim.OnAnimationStopped   += OnAnimationCompleted;
        }

        private void OnDisable()
        {
            if (_anim == null) return;
            _anim.OnAnimationStarted   -= OnAnimationStarted;
            _anim.OnAnimationCompleted -= OnAnimationCompleted;
            _anim.OnAnimationStopped   -= OnAnimationCompleted;
        }

        // -------------------------------------------------------------------------

        private void OnAnimationStarted(string animId)
        {
            // Re-snapshot the Animator in case SetAnimator() was called at runtime.
            if (_animator == null)
            {
                var field = typeof(AnimationManager).GetField("targetAnimator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                _animator = field?.GetValue(_anim) as Animator;
            }

            if (_animator == null) return;

            if (usePunchScale)
            {
                DOTween.Kill(_animator.transform);
                _animator.transform.DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity);
            }

            if (!string.IsNullOrEmpty(blendParamName))
            {
                _blendTween?.Kill();
                float current = _animator.GetFloat(blendParamName);
                _blendTween = DOVirtual.Float(current, 1f, blendInDuration, v =>
                {
                    if (_animator != null) _animator.SetFloat(blendParamName, v);
                }).SetEase(blendEase);
            }
        }

        private void OnAnimationCompleted(string animId)
        {
            if (_animator == null || string.IsNullOrEmpty(blendParamName)) return;

            _blendTween?.Kill();
            float current = _animator.GetFloat(blendParamName);
            _blendTween = DOVirtual.Float(current, 0f, blendOutDuration, v =>
            {
                if (_animator != null) _animator.SetFloat(blendParamName, v);
            }).SetEase(blendEase);
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_DOTWEEN</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/DOTween Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class DotweenAnimationBridge : UnityEngine.MonoBehaviour { }
}
#endif
