using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationManager.Runtime
{
    /// <summary>
    /// Defines a single animation entry that can be played by <see cref="AnimationManager"/>.
    /// </summary>
    [Serializable]
    public class AnimationDefinition
    {
        [Tooltip("Unique identifier for this animation.")]
        public string id;

        [Tooltip("Human-readable display name.")]
        public string displayName;

        [Tooltip("Name of the Animator state or parameter to trigger.")]
        public string stateName;

        [Tooltip("Optional path to a RuntimeAnimatorController asset (Resources-relative).")]
        public string controllerPath;

        [Tooltip("Category tag, e.g. 'idle', 'combat', 'cinematic'.")]
        public string category;

        [Tooltip("Whether this animation loops.")]
        public bool loop;

        [Tooltip("Approximate duration in seconds (used by CrossFade calculations).")]
        public float duration = 1f;
    }

    /// <summary>
    /// JSON root wrapper used when loading animation definitions from StreamingAssets.
    /// </summary>
    [Serializable]
    internal class AnimationManifestJson
    {
        public List<AnimationDefinition> animations = new List<AnimationDefinition>();
    }
}
