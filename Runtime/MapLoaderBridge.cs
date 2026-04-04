#if ANIMATIONMANAGER_MLF
using UnityEngine;
using MapLoaderFramework.Runtime;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Optional bridge between AnimationManager and MapLoaderFramework.
    /// Enable define <c>ANIMATIONMANAGER_MLF</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Calls <see cref="AnimationManager.Stop"/> whenever the active chapter changes
    /// (<see cref="MapLoaderManager.OnChapterChanged"/>), preventing stale animations
    /// from continuing into the new map.
    /// </para>
    /// </summary>
    [AddComponentMenu("AnimationManager/Map Loader Bridge")]
    [DisallowMultipleComponent]
    public class MapLoaderBridge : MonoBehaviour
    {
        private AnimationManager _anim;
        private MapLoaderManager _mlf;

        private void Awake()
        {
            _anim = GetComponent<AnimationManager>() ?? FindFirstObjectByType<AnimationManager>();
            _mlf  = GetComponent<MapLoaderManager>() ?? FindFirstObjectByType<MapLoaderManager>();

            if (_anim == null) Debug.LogWarning("[AnimationManager/MapLoaderBridge] AnimationManager not found.");
            if (_mlf  == null) Debug.LogWarning("[AnimationManager/MapLoaderBridge] MapLoaderManager not found.");
        }

        private void OnEnable()
        {
            if (_mlf != null) _mlf.OnChapterChanged += OnChapterChanged;
        }

        private void OnDisable()
        {
            if (_mlf != null) _mlf.OnChapterChanged -= OnChapterChanged;
        }

        private void OnChapterChanged(int previous, int current)
        {
            _anim?.Stop();
        }
    }
}
#else
namespace AnimationManager.Runtime
{
    /// <summary>No-op stub — enable define <c>ANIMATIONMANAGER_MLF</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("AnimationManager/Map Loader Bridge")]
    public class MapLoaderBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[AnimationManager/MapLoaderBridge] Bridge disabled — add ANIMATIONMANAGER_MLF to Scripting Define Symbols.");
    }
}
#endif
