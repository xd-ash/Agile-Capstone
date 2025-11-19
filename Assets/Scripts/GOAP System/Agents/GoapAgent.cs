using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using static GOAPEnums;

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
    private Unit _unit;

    public List<GoapAction> GetActions => _actions;

    private void Start()
    {
        // Init goal dict creation from list in inspector
        foreach (var g in _goals)
            _weightedGoalsDict.Add(g, g.value);
    }

    // INCOMPLETE make more secure with deleting null actions or actions added in inpsector by hitting +
    #region OnInspectorMethods
    public void GrabActionsFromEnum()
    {
        if (_unit == null) _unit = gameObject.GetComponent<Unit>();

        var temp = GetAllActionsFromFlags(_unit, _goapActionsEnum);
        List<string> tempToString = new List<string>(),
                     actionsToString = new List<string>();

        foreach (var a in temp)
            tempToString.Add(a.ToString());

        if (_actions.Count > 0)
            foreach (var a in _actions)
                if (a != null)
                    actionsToString.Add(a.ToString());

        for (int i = 0; i < actionsToString.Count; i++)
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

    void CompleteAction()
    {
        _currentAction.running = false;
        _currentAction.PostPerform(ref _beliefs);
    }

    void LateUpdate()
    {
        if (_currentAction != null && _currentAction.running) return;

        if (_planner == null || _actionQueue == null)
        {
            _planner = new GoapPlanner();

            var sortedGoals = from entry in _weightedGoalsDict 
                              orderby entry.Value descending 
                              select entry;

            foreach (KeyValuePair<Goal, int> g in sortedGoals)
            {
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
                _weightedGoalsDict.Remove(_currentGoal);
            _planner = null;
        }

        if (_actionQueue != null && _actionQueue.Count > 0)
        {
            _currentAction = _actionQueue.Dequeue();
            if (_currentAction.PrePerform())
            {
                _currentAction.running = true;

                /*Navmesh from tutorial
                if (currentAction.target == null && currentAction.targetTag != "")
                    currentAction.target =  GameObject.FindWithTag(currentAction.targetTag);

                if (currentAction.target != null)
                {
                    currentAction.running = true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position); //set destination for navmesh agent
                }
                */
            }
            else
            {
                _actionQueue = null;
            }
        }
    }
}
