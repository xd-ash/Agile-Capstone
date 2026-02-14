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

        TurnManager.Instance.OnTurnStart += OnThisUnitTurnStart; // add option or something in effect class for end of turn only effects??
    }
    private void OnDestroy()
    {
        if (TurnManager.Instance == null) return;

        TurnManager.Instance.OnTurnStart += OnThisUnitTurnStart;
    }
    public void AddEffect(Action effect, int totalDuration, Guid guid, string effectName = "")
    {
        Effect newEffect = new(effect, totalDuration, guid, effectName);

        if(!_effects.Contains(newEffect)) //list will probably never contain a duplicate since new GUID is created for each effect
            _effects.Add(newEffect);
    }
    private void OnThisUnitTurnStart(Unit unit)
    {
        if (unit != _unit) return;

        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var e = _effects[i];
            e.storedEffect?.Invoke();
            e.turnsRemaining--;
            if (e.turnsRemaining <= 0)
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

        public Effect(Action effect, int totalDuration, Guid guid, string name = "")
        {
            storedEffect = effect;
            turnsRemaining = totalDuration;
            this.guid = guid;
            effectName = name == string.Empty ? guid.ToString() : name;
        }
    }
}
