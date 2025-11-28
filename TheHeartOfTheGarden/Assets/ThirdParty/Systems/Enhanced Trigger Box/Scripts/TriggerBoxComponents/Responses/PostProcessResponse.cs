using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EnhancedTriggerbox.Component
{
    /// <summary>
    /// This response adjusts post-processing effects such as color tint and saturation.
    /// Uses a single targetSaturation value instead of min/max.
    /// </summary>
    [AddComponentMenu("")]
    public class PostProcessResponse : ResponseComponent
    {
        [Header("Post Processing Settings")]
        public Volume targetVolume;
        public Color targetTint = new(0.9f, 0.95f, 1f);

        [Tooltip("Target saturation value (-100 = fully desaturated, 0 = normal)")]
        [Range(-100f, 0f)]
        public float targetSaturation = -100f;

        [Tooltip("Blend value 0..1 applied when this response executes (how strongly the tint/sat is applied)")]
        [Range(0f, 1f)]
        public float blendAmount = 1f;

        [Tooltip("If greater than 0, transition will tween over this duration in seconds")]
        public float blendDuration = 0f;

        private ColorAdjustments colorAdj;
        private Coroutine activeRoutine;

#if UNITY_EDITOR
        public override void DrawInspectorGUI()
        {
            targetVolume = (Volume)UnityEditor.EditorGUILayout.ObjectField(
                new GUIContent("Target Volume", "The volume that contains your post-processing profile."),
                targetVolume, typeof(Volume), true);

            targetTint = UnityEditor.EditorGUILayout.ColorField(
                new GUIContent("Target Tint", "The color tint to blend toward."), targetTint);

            targetSaturation = UnityEditor.EditorGUILayout.Slider(
                new GUIContent("Target Saturation", "Target saturation (-100..0)."), targetSaturation, -100f, 0f);

            blendAmount = UnityEditor.EditorGUILayout.Slider(
                new GUIContent("Blend Amount", "0 = no effect, 1 = full effect."), blendAmount, 0f, 1f);

            blendDuration = UnityEditor.EditorGUILayout.FloatField(
                new GUIContent("Blend Duration", "If > 0, the change will tween over this many seconds."), blendDuration);
        }
#endif

        public override void Validation()
        {
            if (targetVolume == null)
                ShowWarningMessage("You must assign a Volume to apply post-processing changes.");
        }

        public override bool ExecuteAction()
        {
            if (!targetVolume) return false;

            // Grab ColorAdjustments override
            if (colorAdj == null)
            {
                if (!targetVolume.profile.TryGet(out colorAdj))
                {
                    ShowWarningMessage("Target Volume has no ColorAdjustments override.");
                    return false;
                }
            }

            // Stop any running tween
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            // Compute the final blend target (0..1) and start a tween to that blend
            float targetBlend = Mathf.Clamp01(blendAmount);

            if (blendDuration > 0f)
            {
                activeRoutine = StartCoroutine(BlendOverTime(targetBlend, blendDuration));
            }
            else
            {
                // Instant apply
                SetBlend(targetBlend);
            }

            return false; // allow trigger chain to continue
        }

        /// <summary>
        /// Apply the blend immediately. 't' is 0..1 strength of effect.
        /// Saturation lerps from current saturation -> targetSaturation (weighted by t).
        /// ColorFilter lerps from white -> targetTint (weighted by t).
        /// </summary>
        private void SetBlend(float t)
        {
            if (colorAdj == null) return;

            // For saturation: Interpolate between current saturation and the target saturation * t.
            // We'll set finalSaturation as: current -> Lerp(current, targetSaturation, t)
            float currentSat = colorAdj.saturation.value;
            float finalSat = Mathf.Lerp(currentSat, targetSaturation, t);
            colorAdj.saturation.value = finalSat;

            // For color filter: interpolate from current color -> target color * t (relative to white)
            Color currentCol = colorAdj.colorFilter.value;
            Color targetCol = Color.Lerp(Color.white, targetTint, t);
            colorAdj.colorFilter.value = Color.Lerp(currentCol, targetCol, 1f); // immediate to targetCol
        }

        /// <summary>
        /// Smoothly tween from the current values to the desired values (determined by targetBlend).
        /// This reads current runtime values as start points so the tween behaves well if interrupted.
        /// </summary>
        private IEnumerator BlendOverTime(float targetBlend, float duration)
        {
            if (colorAdj == null) yield break;

            float startSat = colorAdj.saturation.value;
            Color startCol = colorAdj.colorFilter.value;

            float desiredSat = Mathf.Lerp(startSat, targetSaturation, targetBlend);
            Color desiredCol = Color.Lerp(Color.white, targetTint, targetBlend);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / duration);
                colorAdj.saturation.value = Mathf.Lerp(startSat, desiredSat, u);
                colorAdj.colorFilter.value = Color.Lerp(startCol, desiredCol, u);
                yield return null;
            }

            // ensure exact final values
            colorAdj.saturation.value = desiredSat;
            colorAdj.colorFilter.value = desiredCol;

            activeRoutine = null;
        }
    }
}
