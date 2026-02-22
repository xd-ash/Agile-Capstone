using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using static GOAPEnums;
using static GOAPDeterminationMethods;
using CardSystem;

[System.Serializable]
public class Goal
{
    [SerializeField, HideInInspector] public string key = "";
    public int value = 0;
    public bool removeOnComplete = false; //removes goal once completed
    public Dictionary<string, int> GetGoal => new Dictionary<string, int>() { { key, value } };

    public Goal(string s, int i, bool r)
    {
        key = s;
        value = i;
        removeOnComplete = r;
    }
}

public class GoapAgent : MonoBehaviour
{
    [Header("Temp Enemy Abilities")]
    public CardAbilityDefinition damageAbility; 
    public CardAbilityDefinition healAbility;
    public int healCharges = 3;
    [Space(15)]

    [SerializeField] private GoapActions _goapActionsEnum;
    [SerializeReference] private List<GoapAction> _actions = new();
    private GoapAction _currentAction;
    private Queue<GoapAction> _actionQueue;

    [SerializeField] private GoapStates _goalsEnum;
    [SerializeReference] private List<Goal> _goals = new();// find better way? used for setting weight/values & remove bool
    private Dictionary<Goal, int> _weightedGoalsDict = new();
    private Goal _currentGoal;

    [SerializeField] private float _actionDelayTime = 1.5f;

    private WorldStates _beliefs = new WorldStates(); //make public or getter/setter if actions needed
    private GoapPlanner _planner;
    [HideInInspector] public Unit unit;
    private Unit _curtarget;

    public bool showDebugMessages = false;

    public List<GoapAction> GetActions => _actions;
    public Unit GetCurrentTarget => _curtarget;

    private void Awake()
    {
        GrabActionsFromEnum();
        GrabGoalsFromEnum();

        foreach (var a in _actions)
            a?.GrabConditionsFromEnums();

        ResetStates();
        /*
        _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
        CheckForAP(unit, ref _beliefs);
        
        // Init goal dict creation from list in inspector
        foreach (var g in _goals)
            _weightedGoalsDict.Add(g, g.value);
        */
    }
    void LateUpdate()
    {
        if (TurnManager.GetCurrentUnit != unit) return;
        if (_currentAction != null && _currentAction.IsRunning) return;

        if (_planner == null || _actionQueue == null)
        {
            _planner = new GoapPlanner(this);
            var sortedGoals = from entry in _weightedGoalsDict
                              orderby entry.Value descending
                              select entry;
            //Debug.Log($"weightGoals count: {_weightedGoalsDict.Count}");

            foreach (KeyValuePair<Goal, int> g in sortedGoals)
            {
                //Debug.Log($"Action (post count): {_actions[0].ToString()}({_actions[0].postConditions.Count})");
                _actionQueue = _planner.Plan(_actions, g.Key.GetGoal, _beliefs);
                if (_actionQueue != null)
                {
                    _currentGoal = g.Key;
                    break;
                }
            }
        }

        if (_actionQueue != null && _actionQueue.Count == 0)
        {
            if (_beliefs.GetStates.ContainsKey(_currentGoal.key) && _currentGoal.removeOnComplete)
                for (int i = _weightedGoalsDict.Count - 1; i >= 0; i--)
                    if (_weightedGoalsDict.ElementAt(i).Key.key == _currentGoal.key)
                        _weightedGoalsDict.Remove(_weightedGoalsDict.ElementAt(i).Key);

            _planner = null;
        }

        if (_actionQueue != null && _actionQueue.Count > 0)
        {
            _currentAction = _actionQueue.Dequeue();
            if (_currentAction.PrePerform(ref _beliefs))
            {
                _currentAction.IsRunning = true;

                if (_currentAction is AttackAction || _currentAction is HealAction)
                    Invoke(nameof(ActionPerformDelay), _actionDelayTime);
                else
                    _currentAction.Perform();
            }
            else
            {
                _actionQueue = null;
            }
        }
    }

    // INCOMPLETE make more secure with deleting null actions or actions added in inpsector by hitting +
    #region OnInspectorMethods
    public void GrabActionsFromEnum()
    {
        if (unit == null) unit = gameObject.GetComponent<Unit>();

        var temp = GetAllActionsFromFlags(this, _goapActionsEnum);
        List<string> tempToString = new List<string>(),
                     actionsToString = new List<string>();

        foreach (var a in temp)
            tempToString.Add(a.ToString());

        if (_actions.Count > 0)
            foreach (var a in _actions)
                if (a != null)
                    actionsToString.Add(a.ToString());

        for (int i = actionsToString.Count - 1; i >= 0 ; i--)
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
        List<string> tempToString = new List<string>();
                     //goalsToString = new List<string>();

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
    //
    public void SetCurrentTarget(Unit target)
    {
        _curtarget = target;
    }
    public void CompleteAction()
    {
        _currentAction.IsRunning = false;
        _currentAction.PostPerform(ref _beliefs);
        GameUIManager.instance.UpdateApText();
        if (!_beliefs.GetStates.ContainsKey(GoapStates.HasAttacked.ToString()))
            CheckForAP(unit, ref _beliefs);
    }

    public void ResetStates() 
    {
        _weightedGoalsDict = new();

        string temp = "weighted dict goals: ";
        // goal dict reset and creation from list in inspector
        foreach (var g in _goals)
        {
            _weightedGoalsDict.Add(g, g.value);
            temp += g.key + ", ";
        }
        //Debug.Log(temp);

        if (unit != null)
        {
            _beliefs = new();

            _beliefs.ModifyState(GoapStates.NoTarget.ToString(), 1);

            if (healCharges > 0)
                _beliefs.ModifyState(GoapStates.CanHeal.ToString(), 1);

            CheckForAP(unit, ref _beliefs);
            CheckIfHealthy(unit, ref _beliefs);

            if (_curtarget == null)
            {
                _beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
                _beliefs.RemoveState(GoapStates.HasLOS.ToString());

                _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
                _beliefs.RemoveState(GoapStates.InRange.ToString());
            }
            else
            {
                CheckIfInRange(this, damageAbility.GetRange, ref _beliefs);
                CheckIfInLOS(this, ref _beliefs);
            }
        }
        /*
        string tempStr = "Beliefs on reset: ";
        foreach (var b in _beliefs.GetStates())
            tempStr += b.Key + ", ";
        Debug.Log(tempStr);
        */
    }

    private void ActionPerformDelay()
    {
        _currentAction.Perform();
    }
    public void ClearPlanner()
    {
        _planner = null;
        _actionQueue = null;
    }
}
