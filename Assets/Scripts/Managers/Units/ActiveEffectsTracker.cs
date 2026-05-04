using System;
using System.Collections.Generic;
using UnityEngine;

public class ActiveEffectsTracker : MonoBehaviour
{
    private Unit _unit;
    [SerializeField] private List<Effect> _effects = new();

    public event Action OnEffectsChanged;

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

    public void AddEffect(Action effect, int totalDuration, Guid guid, bool tickOnStart, string effectName = "", Action onRemoved = null, Sprite icon = null)
    {
        Effect newEffect = new(ref effect, totalDuration, guid, tickOnStart, effectName, onRemoved, icon);

        if (!_effects.Contains(
                newEffect)) //list will probably never contain a duplicate since new GUID is created for each effect
        {
            //replace same effects to avoid stacking dots/hots
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].effectName == newEffect.effectName)
                {
                    _effects[i].onRemoved?.Invoke();
                    _effects.RemoveAt(i);
                }
            }
        _effects.Add(newEffect);
        }
        OnEffectsChanged?.Invoke();
    }
    
    public List<EffectInfo> GetActiveEffectInfos()
    {
        List<EffectInfo> infos = new();
        foreach (var e in _effects)
        {
            if (e.icon == null)
            {
                continue;
            }
            infos.Add(new EffectInfo(e.effectName, e.turnsRemaining, e.icon));            
        }

        return infos;
    }

    private void OnThisUnitEffectsTick(Unit unit, bool isStartOfTurn)
    {
        if (unit != _unit) return;
        
        bool changed = false;

        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var e = _effects[i];
            if (isStartOfTurn != e.tickOnStart) continue;
            
            e.storedEffect?.Invoke();
            e.turnsRemaining--;
            changed = true; 
            if (e.turnsRemaining > 0) continue;
            if (e.effectName == "Stop Movement Effect")
            {
                unit.ToggleCanMove(true);
                //Debug.Log($"Stop movement effect manual unit bool flip occured. Fix me sometime :)");
            }
            e.onRemoved?.Invoke();  
            _effects.Remove(e);
        }
        
        if (changed)
            OnEffectsChanged?.Invoke();
    }

    [System.Serializable]
    private class Effect
    {
        [HideInInspector] public string effectName;
        public Guid guid;
        public Action storedEffect;
        public Action onRemoved;
        public int turnsRemaining;
        public bool tickOnStart;
        public Sprite icon;

        public Effect(ref Action effect, int totalDuration, Guid guid, bool tickOnStart, string name = "", Action onRemoved = null, Sprite icon = null)
        {
            storedEffect = effect;
            turnsRemaining = totalDuration;
            this.guid = guid;
            this.tickOnStart = tickOnStart;
            effectName = name == string.Empty ? guid.ToString() : name;
            this.onRemoved = onRemoved;
            this.icon = icon;
        }
    }
}

// [STATUS_UI] Lightweight read-only data for UI to display
public struct EffectInfo
{
    public string name;
    public int turnsRemaining;
    public Sprite icon;

    public EffectInfo(string name, int turnsRemaining, Sprite icon)
    {
        this.name = name;
        this.turnsRemaining = turnsRemaining;
        this.icon = icon;
    }
}