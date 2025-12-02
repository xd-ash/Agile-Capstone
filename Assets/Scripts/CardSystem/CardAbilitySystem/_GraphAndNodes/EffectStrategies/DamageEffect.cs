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
        [SerializeField] private int missFontSize = 36;
        [Tooltip("Color of the floating 'Misses' text.")]
        [SerializeField] private Color missColor = Color.white;
        [Tooltip("Duration the 'Misses' text stays visible (seconds).")]
        [SerializeField] private float missDuration = 1.0f;
        [Tooltip("Vertical offset above the unit to place the popup (world units).")]
        [SerializeField] private float missYOffset = 1.5f;
        [Tooltip("Vertical speed while fading out (world units per second).")]
        [SerializeField] private float missRiseSpeed = 0.5f;

        [Header("Miss popup rendering")]
        [Tooltip("Sorting layer for miss popup Canvas (create in editor if needed).")]
        [SerializeField] private string missSortingLayer = "UI";
        [Tooltip("Sorting order for miss popup Canvas (higher renders on top).")]
        [SerializeField] private int missSortingOrder = 10000;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null && target.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    int abilRange = (graph as CardAbilityDefinition).RootNode.GetRange;

                    bool hit = CombatMath.RollHit(abilityData.GetUnit, targetUnit, abilRange, out int hitChance, out float roll);

                    _visualsStrategy?.CreateVisualEffect(abilityData, targetUnit); //do effect visuals

                    if (!hit)
                    {
                        // floating 'Miss' text here
                        // Start a coroutine on the target unit to spawn and fade a small world-space UI text above it
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

        // Coroutine: create a simple world-space Canvas with a Text, fade it and destroy.
        private IEnumerator SpawnMissPopup(Unit unit)
        {
            if (unit == null) yield break;

            // root GameObject for popup (create at world root to avoid inheriting unit scale)
            GameObject root = new GameObject("MissPopup");
            // place at world position above unit so popup isn't affected by unit's local scale
            root.transform.SetParent(null, false);
            root.transform.position = unit.transform.position + Vector3.up * missYOffset;
            root.transform.rotation = Quaternion.identity;

            // add world-space canvas
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            // do not assume Camera.main exists; assign if available
            if (Camera.main != null) canvas.worldCamera = Camera.main;
            // ensure sorting override so ordering takes effect
            canvas.overrideSorting = true;
            canvas.sortingLayerName = missSortingLayer;
            canvas.sortingOrder = missSortingOrder;

            var rect = root.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 60f);

            // create Text (legacy UI Text for simplicity)
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(root.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = rect.sizeDelta;

            var text = textGO.AddComponent<Text>();
            text.text = "Misses";
            // try to get a font; fall back gracefully if null
            Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f != null) text.font = f;
            text.fontSize = missFontSize;
            text.color = missColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            // scale so world-space canvas appears at reasonable size and make sure scale is 1
            float scale = 0.01f;
            rect.localScale = Vector3.one * scale;

            float elapsed = 0f;
            Color startColor = text.color;

            while (elapsed < missDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / missDuration);
                // fade out alpha
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                text.color = c;
                // rise up slowly in world space
                root.transform.position += Vector3.up * (missRiseSpeed * Time.deltaTime);
                yield return null;
            }

            UnityEngine.Object.Destroy(root);
        }
    }
}