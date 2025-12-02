using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CardSystem
{
    public enum DamageTypes //not used for much currently
    {
        None,
        Slash,
        Pierce,
        Fire,
        Emotional
    }

    // Concrete harmful effect class to damage a unit instantly or over a duration
    [CreateNodeMenu("Harmful Effects/Damage")]
    public class DamageEffect : HarmfulEffect
    {
        [Header("Miss popup")]
        [Tooltip("Font size used for the floating 'Misses' text.")]
        [SerializeField] private int missFontSize = 48;
        [Tooltip("Color of the floating 'Misses' text.")]
        [SerializeField] private Color missColor = Color.white;
        [Tooltip("Duration the 'Misses' text stays visible (seconds).")]
        [SerializeField] private float missDuration = 1.0f;
        [Tooltip("Vertical offset above the unit to place the popup (world units).")]
        [SerializeField] private float missYOffset = 1.5f;
        [Tooltip("Vertical speed while fading out (world units per second).")]
        [SerializeField] private float missRiseSpeed = 0.5f;

        [Header("Miss popup rendering")]
        [Tooltip("Character size multiplier for the 3D TextMesh (world scale).")]
        [SerializeField] private float missCharacterSize = 0.08f;
        [Tooltip("Optional: if true, popup will face the main camera if present.")]
        [SerializeField] private bool faceCamera = true;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    int abilRange = (graph as CardAbilityDefinition).RootNode.GetRange;

                    bool hit = CombatMath.RollHit(abilityData.GetUnit, targetUnit, abilRange, out int hitChance, out float roll);

                    _visualsStrategy.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                    if (!hit)
                    {
                        // floating 'Miss' text here
                        // Start a coroutine on the target unit to spawn and fade a small world-space 3D TextMesh above it
                        targetUnit.StartCoroutine(SpawnMissPopup(targetUnit));
                        Debug.Log($"[{abilityData.GetUnit}] Attack Missed, Targetted @ {targetUnit}");
                        continue;
                    }

                    if (_hasDuration)
                        targetUnit.StartCoroutine(DoEffectOverTime(targetUnit, _duration, _effectValue));
                    else
                        targetUnit.ChangeHealth(_effectValue, false);
                }
            }

            onFinished();
        }

        // Simplified: Uses a 3D TextMesh so we avoid Canvas / Camera / Font nulls.
        private IEnumerator SpawnMissPopup(Unit unit)
        {
            if (unit == null) yield break;

            // Create a simple world-space TextMesh object (no Canvas involved)
            GameObject go = new GameObject("MissPopup");
            // place at world position above unit (avoid inheriting unit scale)
            go.transform.SetParent(null, false);
            go.transform.position = unit.transform.position + Vector3.up * missYOffset;

            // Add TextMesh for simple 3D text
            var textMesh = go.AddComponent<TextMesh>();
            textMesh.text = "Misses";
            textMesh.fontSize = Mathf.Max(1, missFontSize);
            textMesh.characterSize = Mathf.Max(0.0001f, missCharacterSize);
            textMesh.color = missColor;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            // Optionally orient toward camera if available
            if (faceCamera && Camera.main != null)
            {
                go.transform.rotation = Quaternion.LookRotation(go.transform.position - Camera.main.transform.position);
            }
            else
            {
                go.transform.rotation = Quaternion.identity;
            }

            float elapsed = 0f;
            Color startColor = textMesh.color;

            while (elapsed < missDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / missDuration);

                // fade alpha
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                textMesh.color = c;

                // rise up
                go.transform.position += Vector3.up * (missRiseSpeed * Time.deltaTime);

                // keep facing camera if requested (handles camera movement)
                if (faceCamera && Camera.main != null)
                    go.transform.rotation = Quaternion.LookRotation(go.transform.position - Camera.main.transform.position);

                yield return null;
            }

            UnityEngine.Object.Destroy(go);
        }
    }
}