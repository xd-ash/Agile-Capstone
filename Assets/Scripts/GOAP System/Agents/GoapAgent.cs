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
    [Space(15)]

    [SerializeField] private GoapActions _goapActionsEnum;
    [SerializeReference] private List<GoapAction> _actions = new();
    private GoapAction _currentAction;
    private Queue<GoapAction> _actionQueue;

    [SerializeField] private GoapStates _goalsEnum;
    [SerializeReference] private List<Goal> _goals = new();// find better way? used for setting weight/values & remove bool
    private Dictionary<Goal, int> _weightedGoalsDict = new();
    private Goal _currentGoal;

    private WorldStates _beliefs = new WorldStates(); //make public or getter/setter if actions needed
    private GoapPlanner _planner;
    [HideInInspector] public Unit unit;
    [HideInInspector] public Unit curtarget;

    public List<GoapAction> GetActions => _actions;

    private void Awake()
    {
        //TEMP
        //TurnManager.instance.OnUnitTurnResetStuff += TempTurnStateResets;
    }
    private void OnDestroy()
    {
        //TEMP
        //TurnManager.instance.OnUnitTurnResetStuff -= TempTurnStateResets;
    }
    private void Start()
    {
        GrabActionsFromEnum();
        GrabGoalsFromEnum();

        foreach (var a in _actions)
            a?.GrabConditionsFromEnums();


        _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
        CheckForAP(unit, ref _beliefs);

        // Init goal dict creation from list in inspector
        foreach (var g in _goals)
            _weightedGoalsDict.Add(g, g.value);
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

    public void CompleteAction()
    {
        _currentAction.running = false;
        _currentAction.PostPerform(ref _beliefs);
    }

    public void TempTurnStateResets() 
    {
        _weightedGoalsDict = new();
        
        // goal dict reset and creation from list in inspector
        foreach (var g in _goals)
            _weightedGoalsDict.Add(g, g.value);

        if (curtarget == null) return;
        if (unit != null)
        {
            _beliefs = new();

            if (!CheckIfInRange(this, curtarget, damageAbility.RootNode.GetRange))
                _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            else
                _beliefs.ModifyState(GoapStates.InRange.ToString(), 1);

            CheckForAP(unit, ref _beliefs);
        }
    }
    public void CalcAndRunActions()
    {
        if (TurnManager.GetCurrentUnit != unit) return;
        //if (_currentAction != null && _currentAction.running) return;
        //var tempGoals
        if (_planner == null || _actionQueue == null)
        {
            _planner = new GoapPlanner();
            var sortedGoals = from entry in _weightedGoalsDict
                              orderby entry.Value descending
                              select entry;

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
            if (_currentGoal.removeOnComplete)
                foreach (var kvp in _weightedGoalsDict)
                    if (kvp.Key.key == _currentGoal.key)
                        _weightedGoalsDict.Remove(kvp.Key);

            _planner = null;
        }

        if (_actionQueue != null && _actionQueue.Count > 0)
        {
            _currentAction = _actionQueue.Dequeue();
            if (_currentAction.PrePerform())
            {
                _currentAction.running = true;
                _currentAction.Perform();
            }
            else
            {
                _actionQueue = null;
            }
        }
    }
    void LateUpdate()
    {
        if (TurnManager.GetCurrentUnit != unit) return;
        if (_currentAction != null && _currentAction.running) return;

        if (_planner == null || _actionQueue == null)
        {
            _planner = new GoapPlanner();
            var sortedGoals = from entry in _weightedGoalsDict
                              orderby entry.Value descending
                              select entry;

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
            if (_currentGoal.removeOnComplete)
                for (int i = _weightedGoalsDict.Count - 1; i >= 0; i--)
                    if (_weightedGoalsDict.ElementAt(i).Key.key == _currentGoal.key)
                        _weightedGoalsDict.Remove(_weightedGoalsDict.ElementAt(i).Key);

            _planner = null;
        }

        if (_actionQueue != null && _actionQueue.Count > 0)
        {
            _currentAction = _actionQueue.Dequeue();
            if (_currentAction.PrePerform())
            {
                _currentAction.running = true;
                _currentAction.Perform();
            }
            else
            {
                _actionQueue = null;
            }
        }
    }
}
