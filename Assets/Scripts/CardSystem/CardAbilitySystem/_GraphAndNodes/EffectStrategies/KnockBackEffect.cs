using AStarPathfinding;
using System;
using UnityEngine;
using static IsoMetricConversions;

namespace CardSystem
{
    public class KnockBackEffect : EffectStrategy, IUseEffectValue
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target == null) return;
                var aStar = target.GetComponent<FindPathAStar>();
                var targetUnit = target.GetComponent<Unit>();
                if (aStar == null || targetUnit == null)
                {
                    Debug.LogError($"Knockback failed. No AStar and/or Unit script on gameobject ({target.name})");
                    return;
                }

                Vector2Int casterGridPos = abilityData.AbilityTriggerPos;
                Vector2Int targetGridPos = ConvertToGridFromIsometric(target.transform.localPosition);

                Vector2Int knockbackDir = Vector2Int.zero;

                if (casterGridPos == targetGridPos)
                    knockbackDir = target.GetComponent<FindPathAStar>().PrevPosOnMove - casterGridPos;
                else
                {
                    Vector2Int rawDir = targetGridPos - casterGridPos;
                    Vector2Int absDir = new Vector2Int(Mathf.Abs(rawDir.x), Mathf.Abs(rawDir.y));

                    if (absDir == Vector2Int.one)
                    {
                        int rng = UnityEngine.Random.Range(0, 2);

                        if (rawDir == Vector2Int.one)
                            knockbackDir = rng == 0 ? Vector2Int.up : Vector2Int.right;
                        else if (rawDir == new Vector2Int(-1, 1))
                            knockbackDir = rng == 0 ? Vector2Int.up : Vector2Int.left;
                        else if (rawDir == -Vector2Int.one)
                            knockbackDir = rng == 0 ? Vector2Int.down : Vector2Int.left;
                        else if (rawDir == new Vector2Int(1, -1))
                            knockbackDir = rng == 0 ? Vector2Int.down : Vector2Int.right;
                    }
                    else
                    {
                        if (absDir.x > absDir.y)
                            knockbackDir = rawDir.x > 0 ? Vector2Int.right : Vector2Int.left;
                        else
                            knockbackDir = rawDir.y > 0 ? Vector2Int.up : Vector2Int.down;
                    }
                }

                if (knockbackDir == Vector2Int.zero)
                {
                    Debug.LogError($"Knockback dir is vector2int.zero.");
                    return;
                }

                Vector2Int newPos = targetGridPos + knockbackDir * _effectValue;

                var mapSize = MapCreator.Instance.GetMapSize;
                newPos.x = Mathf.Clamp(newPos.x, 0, mapSize.x - 1);
                newPos.y = Mathf.Clamp(newPos.y, 0, mapSize.y - 1);
                //Debug.Log($"newPos: ({newPos.x},{newPos.y})");

                //if target is already against obstacle/boundary just return
                if (newPos == targetGridPos)
                    return;

                // check for obstacles along path and adjust resulting tile position if any are found
                var lastValidPos = targetGridPos;
                for (int i = 1; i <= _effectValue; i++)
                {
                    var tilePos = targetGridPos + knockbackDir * i;
                    tilePos.x = Mathf.Clamp(tilePos.x, 0, mapSize.x - 1);
                    tilePos.y = Mathf.Clamp(tilePos.y, 0, mapSize.y - 1);

                    if (ByteMapController.Instance.GetByteMap[tilePos.x, tilePos.y] == 2)
                        break;
                    lastValidPos = tilePos;
                }
                //Debug.Log($"lastValidPos: ({lastValidPos.x},{lastValidPos.y})");

                aStar.OnKnockback(lastValidPos);
            }

            onFinished?.Invoke();
        }
    }
}