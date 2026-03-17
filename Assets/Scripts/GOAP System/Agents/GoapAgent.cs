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
    [SerializeField] private GoapAgentSO _agentSO;
    private GoapAgentHueristics _agentHeuristics;

    public int healCharges = 3;

    private List<GoapAction> _actions = new();
    private GoapAction _currentAction;
    private Queue<GoapAction> _actionQueue;

    private List<Goal> _goals = new();

    private Dictionary<Goal, int> _weightedGoalsDict = new();
    private Goal _currentGoal;

    [SerializeField] private float _actionDelayTime = 1.5f;

    private WorldStates _beliefs = new WorldStates(); //make public or getter/setter if actions needed
    private GoapPlanner _planner;
    [HideInInspector] public Unit unit;
    private Unit _curtarget;

    public bool showDebugMessages = false;

    public Unit GetCurrentTarget => _curtarget;

    public GoapAgentSO GetAgentSO => _agentSO;

    //temp?
    public CardAbilityDefinition GetDamageAbility => _agentSO.GetDamageAbility;
    public CardAbilityDefinition GetHealAbility => _agentSO.GetHealAbility;

    private void Awake()
    {
        if (_agentSO == null)
        {
            Debug.LogError($"No GOAP Agent SO Attached to ({gameObject.name})");
            return;
        }
        if (!TryGetComponent(out _agentHeuristics))
            Debug.LogError($"No Goap heuristics attached to agent ({name})");

        _actions = new(_agentSO.GetActions);
        foreach (var a in _actions)
            a.SetAgent(this);
        _goals = new(_agentSO.GetGoals);
        healCharges = _agentSO.GetTotalHealCharges;

        ResetStates();
    }
    void LateUpdate()
    {
        if (TurnManager.GetCurrentUnit != unit) return;
        if (_currentAction != null && _currentAction.IsRunning) return;

        if (_planner == null || _actionQueue == null)
        {
            _planner = new GoapPlanner(this);

            //sort goals based on weight/prio
            var sortedGoals = from entry in _weightedGoalsDict
                              orderby entry.Value descending
                              select entry;

            //pick highest prio goal to plan for first
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

        if (_actionQueue == null)
            if (!CheckForAP(unit, ref _beliefs) || !CheckCanDoAction(unit, _agentSO.GetDamageAbility.GetApCost)|| !CheckCanDoAction(unit, _agentSO.GetHealAbility.GetApCost))
            return;

        // actionqueue is finished
        if (_actionQueue != null && _actionQueue.Count == 0)
        {
            if (_beliefs.GetStates.ContainsKey(_currentGoal.key) && _currentGoal.removeOnComplete)
                for (int i = _weightedGoalsDict.Count - 1; i >= 0; i--)
                {
                    var goal = _weightedGoalsDict.ElementAt(i).Key;
                    if (goal.key != _currentGoal.key) continue;

                    _weightedGoalsDict.Remove(_weightedGoalsDict.ElementAt(i).Key);
                }

            _planner = null;
        }

        // actionqueue is not finished
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
                _actionQueue = null;
        }
    }

    public void SetCurrentTarget(Unit target)
    {
        _curtarget = target;
    }
    public void CompleteAction()
    {
        _currentAction.IsRunning = false;
        _currentAction.PostPerform(ref _beliefs);
        GameUIManager.instance.UpdateApText();
        if (!_beliefs.GetStates.ContainsKey(GoapGoals.KillPlayer.ToString()))
            CheckForAP(unit, ref _beliefs);
    }

    public void ResetStates()
    {
        var agentDesires = _agentHeuristics.GetAgentDesires();

        _weightedGoalsDict = new();
        string tempDebug = "weighted dict goals: ";
        // goal dict reset and creation from list in inspector
        foreach (var g in _goals)
        {
            var tempVal = (float)g.value;
            if (agentDesires.ContainsKey(g.key))
                tempVal = agentDesires[g.key];
            _weightedGoalsDict.Add(g, tempVal);
            tempDebug += g.key + ", ";
        }
        //if (showDebugMessages)
            //Debug.Log(temp);

        if (unit == null) return;

        _beliefs = new();
        _beliefs.ModifyState(GoapStates.NoTarget.ToString(), 1);

        if (healCharges > 0)
            _beliefs.ModifyState(GoapStates.CanHeal.ToString(), 1);

        CheckForAP(unit, ref _beliefs);
        CheckIfHealthy(unit, ref _beliefs);

        _beliefs.ModifyState(GoapStates.CanAttack.ToString(), 1);

        if (_curtarget == null)
        {
            _beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
            _beliefs.RemoveState(GoapStates.HasLOS.ToString());

            _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            _beliefs.RemoveState(GoapStates.InRange.ToString());
        }
        else
        {
            CheckIfInRange(this, _agentSO.GetDamageAbility.GetRange, ref _beliefs);
            CheckIfInLOS(this, ref _beliefs);
        }
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
