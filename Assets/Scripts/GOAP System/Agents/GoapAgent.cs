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
    private GoapAction _prevAction;
    private GoapAction _currentAction;
    private Queue<GoapAction> _actionQueue;

    private List<Goal> _goals = new();

    //private Dictionary<Goal, float> _weightedGoalsDict = new();
    private Dictionary<Goal, Plan> _goalsDict = new();

    private Goal _currentGoal;

    [SerializeField] private float _actionDelayTime = 1.5f;

    private WorldStates _beliefs = new WorldStates(); //make public or getter/setter if actions needed
    private GoapPlanner _planner;
    [HideInInspector] public Unit unit;
    private Unit _enemyTarget;
    private Unit _allyTarget;

    public bool showDebugMessages = false;
    [SerializeField] private int _buildFailCounter = 0;

    public Goal GetCurrentGoal => _currentGoal;
    public Unit GetCurrentTarget => (_currentGoal == null ? null : (_currentGoal.key == GoapGoals.KillPlayer.ToString() ? _enemyTarget : _allyTarget));
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

        //if (_isResetting) return;

        if (_planner == null || _actionQueue == null)
        {
            if (_buildFailCounter > 15)
                SetBuildFailBeliefs();

            PlanForGoals();

            float cheapestCost = float.MaxValue;
            foreach (var element in _goalsDict)
            {
                if (element.Value == null || element.Value.actionQueue == null) continue;

                var cost = element.Value.cheapestCost;
                if (cost >= cheapestCost || element.Value.actionQueue.Count == 0) continue;
                _actionQueue = element.Value.actionQueue;
                _currentGoal = element.Key ?? new("null", 1, true);
                cheapestCost = cost;
                SetCurrentTargets(element.Value.targets[0], element.Value.targets[1]);
            }

            //if (GetCurrentTarget != null && GetCurrentAbility != null)
                //CheckRange(this, GetCurrentAbility.GetRange, ref _beliefs);

            if (_actionQueue == null || _actionQueue.Count == 0)
                _buildFailCounter++;

            if (showDebugMessages && _actionQueue != null)
            {
                string tempStr2 = $"{name}({(_currentGoal == null ? "null" : _currentGoal.key)}) - Chosen Plan: ";
                foreach (GoapAction a in _actionQueue)
                    tempStr2 += $"{a.GetActionName} > ";
                Debug.Log(tempStr2 + $"(Cost: {cheapestCost})");
            }
        }

        if (_actionQueue == null)
        {
            CheckForAP(unit, ref _beliefs);

            if (!CheckCanDoAction(unit, _agentSO.GetHealAbility.GetApCost))
                _beliefs.RemoveState(GoapStates.CanHeal.ToString());
            if (!CheckCanDoAction(unit, _agentSO.GetDamageAbility.GetApCost))
                _beliefs.RemoveState(GoapStates.CanAttack.ToString());
        }

        // actionqueue is finished
        if (_actionQueue != null && _actionQueue.Count == 0)
        {
            if (_beliefs.GetStates.ContainsKey(_currentGoal.key) && _currentGoal.removeOnComplete)
                for (int i = _goalsDict.Count - 1; i >= 0; i--)
                {
                    var goal = _goalsDict.ElementAt(i).Key;
                    if (goal.key != _currentGoal.key) continue;

                    _goalsDict.Remove(_goalsDict.ElementAt(i).Key);
                }

            _planner = null;
        }

        // actionqueue is not finished
        if (_actionQueue != null && _actionQueue.Count > 0)
        { 
            _prevAction = _currentAction;
            _currentAction = _actionQueue.Dequeue();

            if (_currentAction.PrePerform(ref _beliefs))
            {
                if (showDebugMessages)
                    Debug.Log($"Performing-{_currentAction.GetActionName}");

                _currentAction.IsRunning = true;

                if (ShouldDelayAction())
                    Invoke(nameof(PerformAction), _actionDelayTime);
                else
                    PerformAction();
            }
            else
            {
                _buildFailCounter++;
                _actionQueue = null;
            }
        }
    }
    private bool ShouldDelayAction()
    {
        if (_currentAction is ChooseTargetAction /*|| _currentAction is EndTurnAction */ || _currentAction is HideAction)
            return false;

        if (_prevAction == null || _prevAction is ChooseTargetAction || _prevAction is EndTurnAction || _prevAction is HideAction)
            return false;

        // movement action into movement action should have no delay
        if ((_prevAction is MoveInRangeAction || _prevAction is MoveIntoLOSAction || _prevAction is MoveOutOfLOSAction || _prevAction is MoveToRangeAction) &&
            (_currentAction is MoveInRangeAction || _currentAction is MoveIntoLOSAction || _currentAction is MoveOutOfLOSAction || _currentAction is MoveToRangeAction))
            return false;

        return true;
    }
    public void SetCurrentTargets(Unit enemyTarget, Unit allyTarget) 
    {
        _enemyTarget = enemyTarget;
        _allyTarget = allyTarget;

        _beliefs.ModifyState(GoapStates.HasTarget.ToString(), 1);
        _beliefs.RemoveState(GoapStates.NoTarget.ToString());
    }
    public void CompleteAction()
    {
        _currentAction.IsRunning = false;
        _currentAction.PostPerform(ref _beliefs);
        GameUIManager.instance.UpdateApText();
        PostActionChecks();
        //if (!_beliefs.GetStates.ContainsKey(GoapGoals.KillPlayer.ToString()))
        //CheckForAP(unit, ref _beliefs);
    }

    public void PostActionChecks()
    {
        if (!CheckCanDoAction(unit, _agentSO.GetHealAbility.GetApCost) || healCharges == 0)
            _beliefs.RemoveState(GoapStates.CanHeal.ToString());
        if (!CheckCanDoAction(unit, _agentSO.GetDamageAbility.GetApCost))
            _beliefs.RemoveState(GoapStates.CanAttack.ToString());

        CheckForAP(unit, ref _beliefs);
        CheckIfHealthy(unit, ref _beliefs);

        if (GetCurrentTarget != null)
        {
            CheckIfInLOS(this, ref _beliefs);

            if (GetCurrentAbility != null)
                CheckRange(this, GetCurrentAbility.GetRange, ref _beliefs);
        }
    }
    public void ResetStates()
    {
        _buildFailCounter = 0;

        _goalsDict = new();
        _currentGoal = null;
        _planner = null;

        attacksPerformedThisTurn = 0;

        foreach (var g in _goals)
            _goalsDict.Add(g, null);

        if (unit == null) return;

        _beliefs = new();
        _beliefs.ModifyState(GoapStates.NoTarget.ToString(), 1);

        if (healCharges != 0)
            _beliefs.ModifyState(GoapStates.CanHeal.ToString(), 1);

        CheckForAP(unit, ref _beliefs);
        CheckIfHealthy(unit, ref _beliefs);

        _beliefs.ModifyState(GoapStates.CanAttack.ToString(), 1);

        //PlanForGoals();

        if (GetCurrentTarget == null)
        {
            _beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
            _beliefs.RemoveState(GoapStates.HasLOS.ToString());

            _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            _beliefs.RemoveState(GoapStates.InRange.ToString());

            //_beliefs.ModifyState(GoapStates.AtRange.ToString(), 1);
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
        if (showDebugMessages)
            Debug.Log(beliefDebug);
    }
    private void PlanForGoals()
    {
        _planner = new(this);

        for (int i = 0; i < _goalsDict.Count; i++)
        {
            var e = _goalsDict.ElementAt(i);
            var tempPlan = _planner.Plan(_actions, e.Key.GetGoal, _beliefs);
            if (tempPlan == null)
                continue;
            _goalsDict[e.Key] = tempPlan;
        }
    }
    public void SetBuildFailBeliefs()
    {
        _beliefs = new();
        _beliefs.ModifyState(GoapStates.OutOfAP.ToString(), 1);
        Debug.LogWarning($"Excessive build failures. Defaulting to End Turn beliefs.");
        _buildFailCounter = 0;
    }
    public WorldStates GetTempBeliefsGivenGoal(string tempGoal, Unit tempTarget, WorldStates referenceBeliefs)
    {
        WorldStates tempBeliefs = new(referenceBeliefs);

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
            tempBeliefs.ModifyState(GoapStates.HasTarget.ToString(), 1);
            tempBeliefs.RemoveState(GoapStates.NoTarget.ToString());

            var currAbility = tempTarget.GetTeam == unit.GetTeam ? GetHealAbility : GetDamageAbility;
            var b = CheckRange(this, tempTarget, currAbility.GetRange, ref tempBeliefs);
            CheckIfInLOS(this, tempTarget, ref tempBeliefs);
        }
        return tempBeliefs;
    }
    private void PerformAction()
    {
        _currentAction.Perform();
    }
    public void ClearPlanner()
    {
        _planner = null;
        _actionQueue = null;
    }
}
