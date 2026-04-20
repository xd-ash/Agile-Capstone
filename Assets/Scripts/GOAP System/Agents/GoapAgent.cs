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
    [SerializeField] private GoapAgentAbilityController _abilityController;

    public int AttacksPerformedThisTurn { get; set; } = 0;

    private List<GoapAction> _actions = new();
    private GoapAction _prevAction;
    private GoapAction _currentAction;
    private Queue<GoapAction> _actionQueue;

    private List<Goal> _goals = new();

    private Dictionary<Goal, Plan> _goalsDict = new();

    private Goal _currentGoal;

    [SerializeField] private float _actionDelayTime = 1.5f;

    private WorldStates _beliefs = new WorldStates(); //make public or getter/setter if actions needed
    private GoapPlanner _planner;
    public Unit _unit;
    private Unit _enemyTarget;
    private Unit _allyTarget;

    public bool showDebugMessages = false;
    [SerializeField] private int _buildFailCounter = 0;

    public Goal GetCurrentGoal => _currentGoal;
    public Unit GetCurrentTarget => DetermineCurrentTarget();
    public Unit GetAllyTarget => _allyTarget;
    public Unit GetEnemyTarget => _enemyTarget;

    public GoapAgentSO GetAgentSO => _agentSO;
    public Unit GetUnit => _unit;
    public CardAbilityDefinition GetHarmfulAbility => _abilityController?.GetHarmfulAbility;
    public CardAbilityDefinition GetHelpfulAbility => _abilityController?.GetHelpfulAbility;
    public CardAbilityDefinition GetCurrentAbility => _currentGoal != null && _currentGoal.key == GoapGoals.KeepAlliesAlive.ToString() ? GetHelpfulAbility : GetHarmfulAbility;
    public bool CheckCanUseHeal => _abilityController == null ? false : _abilityController.CheckCanUseHeal;
    public bool CheckCanUseAttack => _abilityController == null ? false : _abilityController.CheckCanUseAttack;
    public bool CheckForState(GoapStates state)
    {
        return _beliefs.GetStates.ContainsKey(state.ToString());
    }
    private Unit DetermineCurrentTarget()
    {
        if (_currentGoal == null) return null;
        if (_currentGoal.key == GoapGoals.KillPlayer.ToString() ||
            _currentGoal.key == GoapGoals.StayAliveEnemyFocus.ToString())
            return GetEnemyTarget;
        else if (_currentGoal.key == GoapGoals.StayAliveSelfFocus.ToString())
            return _unit;
        else
            return _allyTarget;
    }
    private void Awake()
    {
        if (_agentSO == null)
        {
            Debug.LogError($"No GOAP Agent SO Attached to ({gameObject.name})");
            return;
        }

        _abilityController = GetComponent<GoapAgentAbilityController>();
        if (_abilityController == null)
        {
            Debug.LogError($"No GOAP Ability Controller Attached to ({gameObject.name})");
            return;
        }
        _abilityController.InitAbilities(_agentSO);

        _unit = GetComponent<Unit>();

        _actions = new();
        foreach (var action in _agentSO.GetActions)
        {
            var clonedAction = action.Clone();
            _actions.Add(clonedAction);
            clonedAction.SetAgent(this);
            clonedAction.GrabConditionsFromEnums();
        }

        _goals = new(_agentSO.GetGoals);

        TurnManager.OnGameStart += ResetStates;
    }
    private void OnDestroy()
    {
        TurnManager.OnGameStart -= ResetStates;
    }
    void LateUpdate()
    {
        if (TurnManager.GetCurrentUnit != _unit) return;
        if (_currentAction != null && _currentAction.IsRunning) return;

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

            if (_actionQueue == null || _actionQueue.Count == 0)
                _buildFailCounter++;

            if (showDebugMessages && _actionQueue != null)
            {
                string tempStr2 = $"{name} ({(_currentGoal == null ? "null" : _currentGoal.key)}) - Chosen Plan: ";
                foreach (GoapAction a in _actionQueue)
                    tempStr2 += $"{a.GetActionName} > ";
                Debug.Log(tempStr2 + $"(Cost: {cheapestCost})");
            }
        }

        if (_actionQueue == null)
        {
            CheckForAP(_unit, ref _beliefs);

            if (_abilityController.GetHelpfulAbility == null || !CheckCanDoAction(_unit, _abilityController.GetHelpfulAbility.GetApCost))
                _beliefs.RemoveState(GoapStates.CanHeal.ToString());
            if (_abilityController.GetHarmfulAbility == null || !CheckCanDoAction(_unit, _abilityController.GetHarmfulAbility.GetApCost))
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
    public void OnUseAbility(CardAbilityDefinition def)
    {
        _abilityController.OnAbilityUse(def);
    }
    public void OnTurnStart()
    {
        ResetStates();
        _abilityController.OnAgentTurnStart();
    }
    private bool ShouldDelayAction()
    {
        if (_currentAction is ChooseTargetAction /*|| _currentAction is EndTurnAction */ || _currentAction is HideAction)
            return false;

        if (_prevAction == null || _prevAction is ChooseTargetAction || _prevAction is EndTurnAction || _prevAction is HideAction)
            return false;

        // movement action into movement action should have no delay
        if ((_prevAction is MoveInRangeAction || _prevAction is MoveIntoLOSAction || _prevAction is MoveOutOfLOSAction || _prevAction is MoveToRangeAction) &&
            (_currentAction is MoveInRangeAction || _currentAction is MoveIntoLOSAction || _currentAction is MoveOutOfLOSAction || _currentAction is MoveToRangeAction || _currentAction is EndTurnAction))
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

        if (showDebugMessages)
        {
            string tempStr = $"Agent: {name} Target: {GetCurrentTarget?.name}:";
            tempStr += $"\nPost Action Beliefs: ";
            foreach (var b in _beliefs.GetStates)
                tempStr += b.Key + ", ";
            //Debug.Log(tempStr);
        }
    }

    public void PostActionChecks()
    {
        if (_abilityController.GetHelpfulAbility == null || !CheckCanDoAction(_unit, _abilityController.GetHelpfulAbility.GetApCost) || !_abilityController.CheckCanUseHeal)
            _beliefs.RemoveState(GoapStates.CanHeal.ToString());
        if (_abilityController.GetHarmfulAbility == null || !CheckCanDoAction(_unit, _abilityController.GetHarmfulAbility.GetApCost) || !_abilityController.CheckCanUseAttack)
            _beliefs.RemoveState(GoapStates.CanAttack.ToString());

        CheckForAP(_unit, ref _beliefs);
        CheckIfHealthy(_unit, ref _beliefs);

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

        AttacksPerformedThisTurn = 0;

        foreach (var g in _goals)
            _goalsDict.Add(g, null);

        if (_unit == null) return;

        _beliefs = new();
        _beliefs.ModifyState(GoapStates.NoTarget.ToString(), 1);

        if (_abilityController.CheckCanUseHeal)
            _beliefs.ModifyState(GoapStates.CanHeal.ToString(), 1);

        CheckForAP(_unit, ref _beliefs);
        CheckIfHealthy(_unit, ref _beliefs);

        _beliefs.ModifyState(GoapStates.CanAttack.ToString(), 1);

        if (GetCurrentTarget == null)
        {
            _beliefs.ModifyState(GoapStates.NoLOS.ToString(), 1);
            _beliefs.RemoveState(GoapStates.HasLOS.ToString());

            _beliefs.ModifyState(GoapStates.OutOfRange.ToString(), 1);
            _beliefs.RemoveState(GoapStates.InRange.ToString());
        }
        else
        {
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

        _goalsDict = new();
        foreach (var g in _goals)
            _goalsDict.Add(g, null);

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
        }
        else
        {
            tempBeliefs.ModifyState(GoapStates.HasTarget.ToString(), 1);
            tempBeliefs.RemoveState(GoapStates.NoTarget.ToString());

            var currAbility = tempTarget.GetTeam == _unit.GetTeam ? _abilityController.GetHelpfulAbility : _abilityController.GetHarmfulAbility;
            if (currAbility != null)
            {
                CheckRange(this, tempTarget, currAbility.GetRange, ref tempBeliefs);
                CheckIfInLOS(this, tempTarget, ref tempBeliefs);
            }
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
