using CardSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static GOAPDeterminationMethods;

[System.Serializable]
public class Goal
{
    [SerializeField, HideInInspector] public string key = "";
    public float value = 0;
    public bool removeOnComplete = false; //removes goal once completed
    public Dictionary<string, float> GetGoal => new Dictionary<string, float>() { { key, value } };

    public Goal(string s, float i, bool r)
    {
        key = s;
        value = i;
        removeOnComplete = r;
    }
}

public class GoapAgent : MonoBehaviour
{
    [SerializeField] private GoapAgentSO _agentSO;

    [Tooltip("Number of times unit can heal per combat. (-1 for infinite heals)")]
    public int healCharges = 3;
    public int attacksPerformedThisTurn = 0;

    private List<GoapAction> _actions = new();
    private GoapAction _currentAction;
    private Queue<GoapAction> _actionQueue;

    private List<Goal> _goals = new();

    private Dictionary<Goal, float> _weightedGoalsDict = new();
    private Goal _currentGoal;

    [SerializeField] private float _actionDelayTime = 1.5f;

    private WorldStates _beliefs = new WorldStates(); //make public or getter/setter if actions needed
    private GoapPlanner _planner;
    [HideInInspector] public Unit unit;
    private Unit _enemyTarget;
    private Unit _allyTarget;

    public bool showDebugMessages = false;

    public Goal GetCurrentGoal => _currentGoal;
    public Unit GetCurrentTarget => (_currentGoal == null ? null : (_currentGoal.key == GoapGoals.KeepAlliesAlive.ToString() ? _allyTarget : _enemyTarget));
    public GoapAgentSO GetAgentSO => _agentSO;

    public CardAbilityDefinition GetDamageAbility => _agentSO?.GetDamageAbility;
    public CardAbilityDefinition GetHealAbility => _agentSO?.GetHealAbility;
    public CardAbilityDefinition GetCurrentAbility => _currentGoal != null && _currentGoal.key == GoapGoals.KillPlayer.ToString() ? _agentSO.GetDamageAbility : _agentSO.GetHealAbility;

    private void Awake()
    {
        if (_agentSO == null)
        {
            Debug.LogError($"No GOAP Agent SO Attached to ({gameObject.name})");
            return;
        }
        //if (!TryGetComponent(out _agentHeuristics))
            //Debug.LogError($"No Goap heuristics attached to agent ({name})");

        _actions = new(_agentSO.GetActions);
        foreach (var a in _actions)
        {
            a.SetAgent(this);
            a.GrabConditionsFromEnums();
        }
        _goals = new(_agentSO.GetGoals);
        healCharges = _agentSO.GetTotalHealCharges;

        TurnManager.OnGameStart += ResetStates;
    }
    private void OnDestroy()
    {
        TurnManager.OnGameStart -= ResetStates;
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

            Dictionary<Goal, Tuple<float, Queue<GoapAction>>> goalQueues = new();
            //pick highest prio goal to plan for first
            foreach (KeyValuePair<Goal, float> g in sortedGoals)
            {
                var tempPlan = _planner.Plan(_actions, g.Key.GetGoal, _beliefs);
                if (tempPlan == null) continue;
                goalQueues.Add(g.Key, tempPlan);
            }

            float cheapestCost = float.MaxValue;
            foreach (var element in goalQueues)
            {
                if (element.Value.Item2 == null) continue;

                var cost = element.Value.Item1;
                if (cost >= cheapestCost) continue; 
                _actionQueue = element.Value.Item2;
                _currentGoal = element.Key ?? new("null", 1, true);
                cheapestCost = cost;
            }
            //
            if (showDebugMessages && _actionQueue != null)
            {
                string tempStr2 = $"{name}({(_currentGoal == null ? "null" : _currentGoal.key)}) - Chosen Plan: ";
                foreach (GoapAction a in _actionQueue)
                    tempStr2 += $"{a.GetActionName} > ";
                Debug.Log(tempStr2 + $"(Cost: {cheapestCost})");
            }
            //
        }

        if (_actionQueue == null)
        {
            CheckForAP(unit, ref _beliefs);

            if (!CheckCanDoAction(unit, _agentSO.GetDamageAbility.GetApCost) && !CheckCanDoAction(unit, _agentSO.GetHealAbility.GetApCost))
            {
                _beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
                _beliefs.RemoveState(GoapStates.HasAP.ToString());
            }
        }

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
                Debug.Log($"Performing-{_currentAction.GetActionName}");

                _currentAction.IsRunning = true;

                if (_currentAction is AttackAction || _currentAction is HealAction)
                {
                    Invoke(nameof(ActionPerformDelay), _actionDelayTime);
                    //Debug.Log($"goal:{(GetCurrentGoal == null ? "null" : GetCurrentGoal.key)}, CurrAbility:{GetCurrentAbility}"); 
                }
                else
                    _currentAction.Perform();
            }
            else
                _actionQueue = null;
        }
    }

    public void SetCurrentTargets(Unit enemyTarget, Unit allyTarget) 
    {
        _enemyTarget = enemyTarget;
        _allyTarget = allyTarget;
    }
    public void CompleteAction()
    {
        SetAgentGoalWeights();
        
        _currentAction.IsRunning = false;
        _currentAction.PostPerform(ref _beliefs);
        GameUIManager.instance.UpdateApText();
        if (!_beliefs.GetStates.ContainsKey(GoapGoals.KillPlayer.ToString()))
            CheckForAP(unit, ref _beliefs);
    }

    public void ResetStates()
    {
        _weightedGoalsDict = new();
        _currentGoal = null;

        attacksPerformedThisTurn = 0;

        string tempDebug = "weighted dict goals: ";
        // goal dict reset and creation from list in inspector
        foreach (var g in _goals)
        {
            _weightedGoalsDict.Add(g, g.value);
            tempDebug += g.key + ", ";
        }
        //if (showDebugMessages) 
        //Debug.Log(temp);

        SetAgentGoalWeights();

        if (unit == null) return;

        _beliefs = new();
        _beliefs.ModifyState(GoapStates.NoTarget.ToString(), 1);

        if (healCharges != 0)
            _beliefs.ModifyState(GoapStates.CanHeal.ToString(), 1);

        CheckForAP(unit, ref _beliefs);
        CheckIfHealthy(unit, ref _beliefs);

        _beliefs.ModifyState(GoapStates.CanAttack.ToString(), 1);

        //Debug.Log($"Goal:{(_currentGoal == null ? "null" : _currentGoal.key)}, target?:{(GetCurrentTarget == null ? "null" : GetCurrentTarget.name)}");

        if (GetCurrentTarget == null)
        {
            _beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
            _beliefs.RemoveState(GoapStates.HasLOS.ToString());

            _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            _beliefs.RemoveState(GoapStates.InRange.ToString());

            _beliefs.ModifyState(GoapStates.AtRange.ToString(), 1);
        }
        else
        {
            //CheckRange(this, _agentSO.GetDamageAbility.GetRange, ref _beliefs);
            var b = CheckRange(this, GetCurrentAbility.GetRange, ref _beliefs);
            CheckIfInLOS(this, ref _beliefs);
        }

        string beliefDebug = "Beliefs: ";
        foreach (var belief in _beliefs.GetStates)
            beliefDebug += $"{belief.Key}, ";
        //Debug.Log(beliefDebug);
    }
    public static WorldStates GetTempBeliefsGivenGoal(GoapAgent agent, string tempGoal, Unit tempTarget, WorldStates referenceBeliefs)
    {
        Unit unit = agent.unit;

        WorldStates tempBeliefs = new(referenceBeliefs);

        //tempBeliefs.ModifyState(GoapStates.NoTarget.ToString(), 1);

        //CheckForAP(unit, ref tempBeliefs);
        //CheckIfHealthy(unit, ref tempBeliefs);

        //tempBeliefs.ModifyState(GoapStates.CanAttack.ToString(), 1);

        if (tempTarget == null)
        {
            tempBeliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
            tempBeliefs.RemoveState(GoapStates.HasLOS.ToString());

            tempBeliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            tempBeliefs.RemoveState(GoapStates.InRange.ToString());

            tempBeliefs.ModifyState(GoapStates.AtRange.ToString(), 1);
        }
        else
        {
            var currAbility = tempTarget.GetTeam == unit.GetTeam ? agent.GetHealAbility : agent.GetDamageAbility;
            var b = CheckRange(agent, tempTarget, currAbility.GetRange, ref tempBeliefs);
            CheckIfInLOS(agent, tempTarget, ref tempBeliefs);
        }
        return tempBeliefs;
    }
    private void SetAgentGoalWeights()
    {
        for (int i = 0; i < _weightedGoalsDict.Count; i++)
        {
            var kvp = _weightedGoalsDict.ElementAt(i);
            _weightedGoalsDict[kvp.Key] = kvp.Value;
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
