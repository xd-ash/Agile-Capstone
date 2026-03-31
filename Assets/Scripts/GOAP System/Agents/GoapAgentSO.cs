using CardSystem;
using System.Collections.Generic;
using UnityEngine;
using static GOAPEnums;

[CreateAssetMenu(fileName = "GoapAgentSO", menuName = "GOAP SOs/Goap Agent SO")]
public class GoapAgentSO : ScriptableObject
{
    [Header("Agent Card Abilities")]
    [SerializeField] private CardAbilityDefinition _damageAbility; //swap to array?
    [SerializeField] private CardAbilityDefinition _healAbility; //swap to array?
    [SerializeField] private int _totalHealCharges = 3; //make better

    //private CardAbilityDefinition[] _damageAbilities;
    //private CardAbilityDefinition[] _healAbilities;

    [Header("Goap Goals & Actions")]
    [SerializeField] private GoapGoals _goalsEnum;
    [SerializeReference] private List<Goal> _goals = new();
    [SerializeField] private GoapActions _goapActionsEnum;
    [SerializeReference] private List<GoapAction> _actions = new();

    public CardAbilityDefinition GetDamageAbility => _damageAbility;
    public CardAbilityDefinition GetHealAbility => _healAbility;
    public int GetTotalHealCharges => _totalHealCharges;

    public List<Goal> GetGoals => _goals;
    public List<GoapAction> GetActions => _actions;

    // Make more secure with deleting null actions or actions added in inpsector by hitting +
    #region OnInspectorMethods
    public void GrabActionsFromEnum()
    {
        var temp = GetAllActionsFromFlags(_goapActionsEnum);
        List<string> tempToString = new List<string>(),
                     actionsToString = new List<string>();

        foreach (var a in temp)
            tempToString.Add(a.ToString());

        if (_actions.Count > 0)
            foreach (var a in _actions)
                if (a != null)
                    actionsToString.Add(a.ToString());

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
                    if (a.ToString() == b.ToString())
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
