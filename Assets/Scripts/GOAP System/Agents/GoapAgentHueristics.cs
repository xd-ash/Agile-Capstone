using AStarPathfinding;
using System.Collections.Generic;
using UnityEngine;
using static AStarPathfinding.FindPathAStar;

public class GoapAgentHueristics : MonoBehaviour
{
    private Unit _unit;
    private GoapAgent _agent;
    private GoapAgentSO _so;

    private int _maxDetectDistance;
    private float _selfPreservation,
                  _aggression,
                  _altruisism;
    
    private void Awake()
    {
        if (!TryGetComponent(out _unit) || !TryGetComponent(out _agent))
        {
            Debug.LogError("Goap Hueristics Script serparated from unit/agent script");
            return;
        }

        _so = _agent.GetAgentSO;
        _maxDetectDistance = _unit.GetMaxAP + Mathf.Max(_agent.GetDamageAbility.GetRange, _agent.GetHealAbility.GetRange);
    }
    public Dictionary<string, float> GetAgentDesires()
    {
        CalculateDesires(TurnManager.GetUnitTurnOrder.ToArray());
        Debug.Log($"sp:{_selfPreservation}, ag:{_aggression}, alt:{_altruisism}");
        return new() { { GoapGoals.StayAlive.ToString(), _selfPreservation },
                       { GoapGoals.KillPlayer.ToString(), _aggression },
                       { GoapGoals.KeepAlliesAlive.ToString(), _altruisism } };
    }

    private void CalculateDesires(Unit[] units)
    {
        var closestUnits = GetClosestUnits(units);
        Unit closestEnemy = closestUnits[0];
        Unit closestAlly = closestUnits[1];//find lowest health ally or average of healths?
        //set agent target here?

        var distToEnemy = CalculatePath(_unit.transform, closestEnemy.transform).Count;
        var distToAlly = CalculatePath(_unit.transform, closestAlly.transform).Count;

        float agentHealthFactor = 0, enemyDistFactorSP = 0,
              enemyDistFactorA = 0, enemyHealthFactor = 0,
              allyDistFactor = 0, allyHealthFactor = 0;

        agentHealthFactor = ((float)_unit.GetMaxHealth - (float)_unit.GetHealth) / (float)_unit.GetMaxHealth;
        enemyDistFactorSP = ((float)_maxDetectDistance - (float)distToEnemy) / (float)_maxDetectDistance;
        enemyDistFactorA = (float)distToEnemy / (float)_maxDetectDistance;
        enemyHealthFactor = ((float)closestEnemy.GetMaxHealth - (float)closestEnemy.GetHealth) / (float)closestEnemy.GetMaxHealth;
        allyDistFactor = ((float)_maxDetectDistance - (float)distToAlly) / (float)_maxDetectDistance;
        allyHealthFactor = ((float)closestAlly.GetMaxHealth - (float)closestAlly.GetHealth) / (float)closestAlly.GetMaxHealth;

        _selfPreservation = agentHealthFactor * _so.GetAgentHealthWeight + enemyDistFactorSP * _so.GetEnemyDistanceWeight;
        _aggression = enemyDistFactorA * _so.GetEnemyDistanceWeight + enemyHealthFactor * _so.GetEnemyHealthWeight;
        _altruisism = allyHealthFactor * _so.GetAllyHealthWeight + allyDistFactor * _so.GetAllyDistanceWeght;
    }

    private Unit[] GetClosestUnits(Unit[] units)
    {
        var thisTeam = _unit.GetTeam;
        float closestEnemyDist = float.MaxValue,
              closestAllyDist =  float.MaxValue;
        Unit closestEnemy = null, closestAlly = null;

        foreach (var unit in units)
        {
            if (unit == null) continue;

            var dist = Vector3.Distance(unit.transform.position, transform.position);

            if (unit.GetTeam == thisTeam)
            {
                if (dist > closestAllyDist) continue;
                closestAllyDist = dist;
                closestAlly = unit;
            }
            else
            {
                if (dist > closestEnemyDist) continue;
                closestEnemyDist = dist;
                closestEnemy = unit;
            }
        }
        return new Unit[2] { closestEnemy,  closestAlly };
    }
    private Unit GetLowestHealthAlly(Unit[] units)
    {
        var thisTeam = _unit.GetTeam;
        float lowestHealthVal = -1;
        Unit lowestHealthUnit = null;

        foreach(var unit in units)
        {
            if (unit.GetTeam != thisTeam) continue;
            if (unit.GetHealth > lowestHealthVal) continue;
            lowestHealthVal = unit.GetHealth;
            lowestHealthUnit = unit;
        }

        return lowestHealthUnit;
    }
}
