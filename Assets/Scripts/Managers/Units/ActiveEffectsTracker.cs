using System;
using System.Collections.Generic;
using UnityEngine;

public class ActiveEffectsTracker : MonoBehaviour
{
    private Unit _unit;
    [SerializeField] private List<Effect> _effects = new();

    private void Awake()
    {
        if (!TryGetComponent(out _unit))
            Debug.LogError($"No Unit script attached to ActiveEffectsTracker of {this.transform.name}.");

        if (TurnManager.Instance == null) return;

        TurnManager.Instance.OnTurnStart += (x) => OnThisUnitEffectsTick(x, true);
        TurnManager.Instance.OnTurnEnd += (x) => OnThisUnitEffectsTick(x, false);
    }
    private void OnDestroy()
    {
        if (TurnManager.Instance == null) return;

        TurnManager.Instance.OnTurnStart -= (x) => OnThisUnitEffectsTick(x, true);
        TurnManager.Instance.OnTurnEnd -= (x) => OnThisUnitEffectsTick(x, false);
    }

    public void AddEffect(Action effect, int totalDuration, Guid guid, bool tickOnStart, string effectName = "")
    {
        Effect newEffect = new(ref effect, totalDuration, guid, tickOnStart, effectName);

        if (!_effects.Contains(newEffect)) //list will probably never contain a duplicate since new GUID is created for each effect
            _effects.Add(newEffect);
    }

    private void OnThisUnitEffectsTick(Unit unit, bool isStartOfTurn)
    {
        if (unit != _unit) return;

        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var e = _effects[i];
            if (isStartOfTurn != e.tickOnStart) continue;
            
            e.storedEffect?.Invoke();
            e.turnsRemaining--;
            if (e.turnsRemaining > 0) continue;
            if (e.effectName == "Stop Movement Effect")
            {
                unit.ToggleCanMove(true);
                //Debug.Log($"Stop movement effect manual unit bool flip occured. Fix me sometime :)");
            }
            _effects.Remove(e);
        }
    }

    [System.Serializable]
    private class Effect
    {
        [HideInInspector] public string effectName;
        public Guid guid;
        public Action storedEffect;
        public int turnsRemaining;
        public bool tickOnStart;

        public Effect(ref Action effect, int totalDuration, Guid guid, bool tickOnStart, string name = "")
        {
            storedEffect = effect;
            turnsRemaining = totalDuration;
            this.guid = guid;
            this.tickOnStart = tickOnStart;
            effectName = name == string.Empty ? guid.ToString() : name;
        }
    }
}
