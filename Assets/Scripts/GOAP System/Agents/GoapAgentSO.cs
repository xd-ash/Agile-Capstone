using CardSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static GOAPEnums;

[System.Serializable]
public class GoapAgentAbility
{
    [HideInInspector] public string abilityName;
    public CardAbilityDefinition ability;
    public int cooldownInTurns = 0;
    [Tooltip("Max number of times the abilty can be used per combat. (-1 for unlimited use)")]
    public int maxUses = -1;

    public int GetRange => ability == null ? 0 : ability.GetRange;
    public int GetCost => ability == null ? 0 : ability.GetApCost;

    public GoapAgentAbility()
    {
        cooldownInTurns = 0;
        maxUses = -1;
    }
    public GoapAgentAbility(GoapAgentAbility refAbility)
    {
        ability = refAbility.ability;
        cooldownInTurns = refAbility.cooldownInTurns;
        maxUses = refAbility.maxUses;
    }
}

[CreateAssetMenu(fileName = "GoapAgentSO", menuName = "GOAP SOs/Goap Agent SO")]
public class GoapAgentSO : ScriptableObject
{
    [Header("Agent Card Abilities")]
    [SerializeField] private GoapAgentAbility[] _harmfulAbilities;
    [SerializeField] private GoapAgentAbility[] _helpfulAbilities;

    [Header("Goap Goals & Actions")]
    [SerializeField] private GoapGoals _goalsEnum;
    [SerializeReference] private List<Goal> _goals = new();
    [SerializeField] private GoapActions _goapActionsEnum;
    [SerializeReference] private List<GoapAction> _actions = new();

    public List<Goal> GetGoals => _goals;
    public List<GoapAction> GetActions => _actions;
    public GoapAgentAbility[] GetHarmfulAbilities => _harmfulAbilities;
    public GoapAgentAbility[] GetHelpfulAbilities => _helpfulAbilities;
    public void SetAbilityNames()
    {
        if (_harmfulAbilities != null)
        {
            for (int i = 0; i < _harmfulAbilities.Length; i++)
            {
                var ability = _harmfulAbilities[i].ability;
                if (ability == null || _harmfulAbilities[i].abilityName == ability.name) continue;
                _harmfulAbilities[i].abilityName = ability.name;
            }
        }

        if (_helpfulAbilities != null)
        {
            for (int i = 0; i < _helpfulAbilities.Length; i++)
            {
                var ability = _helpfulAbilities[i].ability;
                if (ability == null || _helpfulAbilities[i].abilityName == ability.name) continue;
                _helpfulAbilities[i].abilityName = ability.name;
            }
        }
    }

    // Make more secure with deleting null actions or actions added in inpsector by hitting +
    #region OnInspectorMethods
    public void GrabActionsFromEnum()
    {
        var temp = GetAllActionsFromFlags(_goapActionsEnum);
        List<string> tempToString = new List<string>(),
                     actionsToString = new List<string>();

        foreach (var a in temp)
            tempToString.Add(a.GetActionName);

        if (_actions.Count > 0)
            foreach (var a in _actions)
                if (a != null)
                    actionsToString.Add(a.GetActionName);

        for (int i = actionsToString.Count - 1; i >= 0; i--)
        {
            if (actionsToString[i] == null)
            {
                _actions.RemoveAt(i);
                continue;
            }

            if (!tempToString.Contains(actionsToString[i]) ||
                actionsToString[i] == string.Empty)
                _actions.RemoveAt(i);
        }

        if (_actions.Count == 0)
            foreach (var a in temp)
                _actions.Add(a);
        else
        {
            foreach (var a in temp)
            {
                bool actionPresent = false;

                foreach (var b in _actions)
                    if (a.GetActionName == b.GetActionName)
                    {
                        actionPresent = true;
                        break;
                    }

                if (!actionPresent)
                    _actions.Add(a);
            }
        }
    }
    public void GrabGoalsFromEnum()
    {
        var temp = GetAllStatesFromFlags(_goalsEnum);
        List<string> tempToString = new();

        foreach (var s in temp)
            tempToString.Add(s.key);

        for (int i = _goals.Count - 1; i >= 0; i--)
        {
            if (_goals[i] == null)
            {
                _goals.RemoveAt(i);
                continue;
            }

            if (!tempToString.Contains(_goals[i].key) || _goals[i].key == string.Empty)
                _goals.RemoveAt(i);
        }

        if (_goals.Count == 0)
            foreach (var s in temp)
                _goals.Add(new Goal(s.key, s.value, false));
        else
        {
            foreach (var s in temp)
            {
                bool goalPresent = false;

                foreach (var g in _goals)
                    if (s.key == g.key)
                    {
                        goalPresent = true;
                        break;
                    }

                if (!goalPresent)
                    _goals.Add(new Goal(s.key, s.value, false));
            }
        }
    }
    #endregion
}
