#if ANIMATIONMANAGER_REALTOON
using UnityEngine;
using DG.Tweening;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge that applies RealToon Pro smear / motion-blur shader properties via
    /// <see cref="MaterialPropertyBlock"/> on the target character's renderers during animation
    /// playback driven by <see cref="AnimationManager"/>.
    /// Enable define <c>ANIMATIONMANAGER_REALTOON</c> in Player Settings › Scripting Define Symbols.
    /// Requires <b>RealToon Pro</b>.
    /// <para>
    /// On <see cref="AnimationManager.OnAnimationStarted"/> the smear intensity is tweened up to
    /// <see cref="smearIntensity"/>; on <see cref="AnimationManager.OnAnimationCompleted"/> or
    /// <see cref="AnimationManager.OnAnimationStopped"/> it is tweened back to zero.
    /// Assign <see cref="targetRoot"/> to the root of the character hierarchy containing the
    /// Renderers using a RealToon Pro shader.
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/RealToon Bridge")]
    [DisallowMultipleComponent]
    public class RealToonAnimationBridge : MonoBehaviour
    {
        [Header("Target Renderers")]
        [Tooltip("Root transform of the character whose RealToon renderers will receive smear properties. " +
                 "Leave unassigned to search via AnimationManager's target Animator.")]
        [SerializeField] private Transform targetRoot;

        [Header("Smear")]
        [Tooltip("Peak smear intensity applied while the animation is playing (0 = off).")]
        [Range(0f, 1f)]
        [SerializeField] private float smearIntensity = 0.6f;

        [Tooltip("Duration for smear to ramp up at animation start.")]
        [SerializeField] private float smearInDuration = 0.1f;

        [Tooltip("Duration for smear to ramp down at animation end.")]
        [SerializeField] private float smearOutDuration = 0.2f;

        [Tooltip("DOTween ease for smear ramp-up.")]
        [SerializeField] private Ease smearInEase = Ease.OutCubic;

        [Tooltip("DOTween ease for smear ramp-down.")]
        [SerializeField] private Ease smearOutEase = Ease.InCubic;

        [Header("Outline Flash")]
        [Tooltip("When true, briefly widens the RealToon outline at animation start for an impact flash.")]
        [SerializeField] private bool useOutlineFlash = true;

        [Tooltip("Outline width during the impact flash.")]
        [Range(0f, 0.02f)]
        [SerializeField] private float flashOutlineWidth = 0.007f;

        [Tooltip("Duration for the outline flash to peak and return.")]
        [SerializeField] private float flashDuration = 0.15f;

        // RealToon Pro shader property IDs
        private static readonly int PropSmearIntensity = Shader.PropertyToID("_SmearIntensity");
        private static readonly int PropOutlineWidth   = Shader.PropertyToID("_OutlineWidth");

        // -------------------------------------------------------------------------

        private AnimationManager     _anim;
        private Renderer[]           _renderers;
        private MaterialPropertyBlock _mpb;
        private Tween                _smearTween;

        private void Awake()
        {
            _anim = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            _mpb  = new MaterialPropertyBlock();

            if (_anim == null)
            {
                Debug.LogWarning("[AnimationManager/RealToonAnimationBridge] AnimationManager not found.");
                return;
            }

            _renderers = ResolveRenderers();
        }

        private void OnEnable()
        {
            if (_anim == null) return;
            _anim.OnAnimationStarted   += OnAnimationStarted;
            _anim.OnAnimationCompleted += OnAnimationEnded;
            _anim.OnAnimationStopped   += OnAnimationEnded;
        }

        private void OnDisable()
        {
            if (_anim == null) return;
            _anim.OnAnimationStarted   -= OnAnimationStarted;
            _anim.OnAnimationCompleted -= OnAnimationEnded;
            _anim.OnAnimationStopped   -= OnAnimationEnded;

            _smearTween?.Kill();
            ApplySmear(0f, 0f);
        }

        // -------------------------------------------------------------------------

        private void OnAnimationStarted(string animId)
        {
            // Re-resolve renderers in case the character was swapped at runtime.
            _renderers = ResolveRenderers();

            _smearTween?.Kill();
            _smearTween = DOVirtual.Float(0f, smearIntensity, smearInDuration, v =>
            {
                ApplySmear(v, 0f);
            }).SetEase(smearInEase);

            if (useOutlineFlash)
            {
                DOVirtual.Float(0f, flashOutlineWidth, flashDuration * 0.4f, w =>
                {
                    ApplySmear(-1f, w); // -1 means "don't touch smear"
                }).SetEase(Ease.OutSine)
                  .OnComplete(() =>
                  {
                      DOVirtual.Float(flashOutlineWidth, 0f, flashDuration * 0.6f, w =>
                      {
                          ApplySmear(-1f, w);
                      }).SetEase(Ease.InSine);
                  });
            }
        }

        private void OnAnimationEnded(string animId)
        {
            _smearTween?.Kill();
            float current = ReadSmear();
            _smearTween = DOVirtual.Float(current, 0f, smearOutDuration, v =>
            {
                ApplySmear(v, 0f);
            }).SetEase(smearOutEase);
        }

        // -------------------------------------------------------------------------

        /// <summary>
        /// Apply shader properties to all tracked renderers.
        /// Pass <c>smear &lt; 0</c> to skip writing the smear property (outline-only update).
        /// Pass <c>outline &lt;= 0</c> to skip writing the outline property (smear-only update).
        /// </summary>
        private void ApplySmear(float smear, float outline)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                if (smear >= 0f)
                    _mpb.SetFloat(PropSmearIntensity, smear);
                if (outline > 0f)
                    _mpb.SetFloat(PropOutlineWidth, outline);
                r.SetPropertyBlock(_mpb);
            }
        }

        private float ReadSmear()
        {
            if (_renderers == null || _renderers.Length == 0) return 0f;
            var r = _renderers[0];
            if (r == null) return 0f;
            r.GetPropertyBlock(_mpb);
            return _mpb.GetFloat(PropSmearIntensity);
        }

        private Renderer[] ResolveRenderers()
        {
            if (targetRoot != null)
                return targetRoot.GetComponentsInChildren<Renderer>(false);

            // Fall back to the Animator's transform bound to AnimationManager.
            var field = typeof(AnimationManager).GetField("targetAnimator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field?.GetValue(_anim) is Animator animator && animator != null)
                return animator.GetComponentsInChildren<Renderer>(false);

            return System.Array.Empty<Renderer>();
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_REALTOON</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/RealToon Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class RealToonAnimationBridge : UnityEngine.MonoBehaviour { }
}
#endif
