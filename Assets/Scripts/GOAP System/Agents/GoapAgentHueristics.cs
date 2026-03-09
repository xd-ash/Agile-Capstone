using System;
using UnityEngine;

public class GoapAgentHueristics : MonoBehaviour
{
    private Unit _thisUnit;

    [Tooltip("Sum of all weights must add up to 1")]
    [SerializeField] private float _healthWeight,
                                   _enemyDistanceWeight,
                                   _enemyHealthWeight,
                                   _allyHealthWeight,
                                   _allyDistanceWeght;

    private float _maxNeed = 1;
    [SerializeField] private float _selfPreservation, 
                                   _aggression,
                                   _altruistism;

    [Tooltip("Sum of all weights must add up to 1")]
    [SerializeField] private float _selfPreservationWeight,
                                   _aggressionWeight,
                                   _altruisticWeight;

    [SerializeField] private float _attackDesire, 
                                   _stayAliveDesire, 
                                   _helpAllyDesire;

    private void Awake()
    {
        if (!TryGetComponent(out _thisUnit))
            Debug.Log("Hueristics Mono serparated from unit script0");
    }

    private float CalculateDesires(Unit[] units)
    {
        return 0;

        //get closest ally & enemy, as well ally healths
        //find lowest health ally or average of healths
        //calc distances to closest ally & enemy (use tile dist or raw?)

        //if _healthweight over 0
            // healthFactor = (maxhealth - health) / maxhealth (lower health = greater factor value)
            
        //if _enemyDistanceWeight over 0
            // enemyDisFactor = (maxRange? - distToClosestEnemy) / maxRange? (closer to enemy = greater factor value)

        //if _enemyHealthWeight over 0
            // enemyHealthFactor = (maxhealth - health) / maxhealth (lower health = greater factor value)

        //if _allyHealthWeight over 0
            // allyHealthFactor = (maxhealth - health) / maxhealth (lower health = greater factor value)

        //if _allyDistanceWeight over 0
            // allyDistFactor = (maxRange? - distToClosestAlly) / maxRange? (closer to enemy = greater factor value)

        // selfPreserve factor = selfPreservation / maxDesire
        // aggression factor = aggression / maxDesire
        // altruistic factor = altruistic / maxDesire

        // stayAliveDesire = healthFactor * healthWeight + enemyDistfactor * enemyDistWeight
        // aggressionDesire = enemydistFactor * enemyDistWeight + enemyHealthFactor * enemyHealth Weight
        // altruismDesire = allyHealthFactor * allyHealthWeight + allyDistFactor * allyDistWeight
    }
    private Unit[] GetClosestUnits(Unit[] units)
    {
        var thisTeam = _thisUnit.GetTeam;
        float closestEnemyDist = float.MaxValue, 
              closestAllyDist =  float.MaxValue;
        Unit closestEnemy = null, closestAlly = null;

        foreach (var unit in units)
        {
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
        var thisTeam = _thisUnit.GetTeam;
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
