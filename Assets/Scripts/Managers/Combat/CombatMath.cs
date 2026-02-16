using UnityEngine;
using AStarPathfinding;
using static IsoMetricConversions;
using CardSystem; // <-- add this so CombatMath can see CardAbilityDefinition

public static class CombatMath
{
    // Default fallback numbers if no card is provided
    private const int _defaultHitChance = 80;
    private const int _defaultMinHitChance = 10;
    private const int _defaultMaxHitChance = 95;
    private const int _defaultPenaltyPerTile = 5;
    private const float _defaultGlobalMultiplier = 1f;
    private const int _defaultGlobalFlatBonus = 0;

    // New: Card-based overloads
    public static int GetHitChance(Vector3 attackerPos, Unit target, CardAbilityDefinition cardDef)
    {
        int range = cardDef != null ? cardDef.GetRange : 1;
        return GetHitChance(attackerPos, target, range, cardDef);
    }

    public static bool RollHit(Vector3 attackerPos, Unit target, CardAbilityDefinition cardDef)
    {
        //quick exit added as a fix for traps
        if (cardDef != null && cardDef.GetIgnoreLOS)
            return true;

        int range = cardDef != null ? cardDef.GetRange : 1;
        int hitChance = GetHitChance(attackerPos, target, range, cardDef);

        if (hitChance == 100)
            return true;
        /*if (hitChance <= 0)
        {
            //roll = 100f;
            return false;
        }*/
       
        int roll = (int)Random.Range(0f, 100f);
        bool result = roll <= hitChance;
        if (!result)
            target.GetFloatingText?.SpawnFloatingText("MISS", TextPresetType.MissTextPreset);
        return result;
    }

    /*public static int GetHitChance(Vector3 attackerPos, Unit target, int abilityRange)
        => GetHitChance(attackerPos, target, abilityRange, null);

    public static bool RollHit(Vector3 attackerPos, Unit target, int abilityRange, out int hitChance, out float roll)
    {
        hitChance = GetHitChance(attackerPos, target, abilityRange, null);

        if (hitChance <= 0)
        {
            roll = 100f;
            return false;
        }

        roll = Random.Range(0f, 100f);
        bool result = roll <= hitChance;
        if (!result)
            target.GetFloatingText?.SpawnFloatingText("MISS", TextPresetType.MissTextPreset);

        return result;
    }*/

    private static int GetHitChance(Vector3 attackerPos, Unit target, int abilityRange, CardAbilityDefinition cardDef)
    {
        if (target == null || !HasLineOfSight(attackerPos, target))
        {
            return 0;
        }

        int baseHitChance = cardDef != null ? cardDef.GetBaseHitChance : _defaultHitChance;
        
        if (baseHitChance == 100)
            return 100;

        int minHitChance = cardDef != null ? cardDef.GetMinHitChance : _defaultMinHitChance;
        int maxHitChance = cardDef != null ? cardDef.GetMaxHitChance : _defaultMaxHitChance;

        int penaltyPerTile = cardDef != null ? cardDef.GetHitPenaltyPerTile : _defaultPenaltyPerTile;
        float multiplier = cardDef != null ? cardDef.GetAccuracyMultiplier : _defaultGlobalMultiplier;
        int flatBonus = cardDef != null ? cardDef.GetAccuracyFlatBonus : _defaultGlobalFlatBonus;

        Vector2Int attackerCell = ConvertToGridFromIsometric(attackerPos);
        Vector2Int targetCell = ConvertToGridFromIsometric(target.transform.localPosition);

        int distance = Mathf.Abs(attackerCell.x - targetCell.x) + Mathf.Abs(attackerCell.y - targetCell.y); // Manhattan

        int extraDistance = Mathf.Max(0, distance - abilityRange);

        int distancePenalty;
        if (extraDistance > 0 && abilityRange <= 1)
        {
            distancePenalty = baseHitChance;
        }
        else
        {
            distancePenalty = extraDistance * penaltyPerTile;
        }

        int rawHitChance = baseHitChance - distancePenalty;

        float scaled = rawHitChance * multiplier;
        int final = Mathf.RoundToInt(scaled) + flatBonus;

        final = Mathf.Clamp(final, minHitChance, maxHitChance);
        return final;
    }
    public static bool HasLineOfSight(Vector3 attackerPos, Unit target)
    {
        if (target == null)
        {
            return false;
        }

        if (MapCreator.Instance == null)
        {
            return true; // Fail open
        }

        Vector2Int attackerCell = ConvertToGridFromIsometric(attackerPos);
        Vector2Int targetCell = ConvertToGridFromIsometric(target.transform.localPosition);

        if (attackerCell == targetCell)
            return true;
        else
            return HasLineOfSight(attackerCell, targetCell);
    }

    public static bool HasLineOfSight(Vector2Int startCell, Vector2Int endCell)
    {
        byte[,] map = MapCreator.Instance.GetByteMap;
        if (map == null)
        {
            return true;
        }

        int startX = startCell.x;
        int startY = startCell.y;
        int endX = endCell.x;
        int endY = endCell.y;

        int deltaX = Mathf.Abs(endX - startX);
        int deltaY = Mathf.Abs(endY - startY);

        int stepX = (startX < endX) ? 1 : -1;
        int stepY = (startY < endY) ? 1 : -1;

        int error = deltaX - deltaY;

        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);

        int currentX = startX;
        int currentY = startY;

        while (true)
        {
            bool isStartCell = (currentX == startX && currentY == startY);
            bool isEndCell = (currentX == endX && currentY == endY);

            if (!isStartCell && !isEndCell)
            {
                if (!IsTransparent(currentX, currentY, map, mapWidth, mapHeight))
                {
                    return false;
                }
            }

            if (isEndCell)
            {
                break;
            }

            int errorTwice = 2 * error;

            if (errorTwice > -deltaY)
            {
                error -= deltaY;
                currentX += stepX;
            }

            if (errorTwice < deltaX)
            {
                error += deltaX;
                currentY += stepY;
            }
        }
        return true;
    }

    private static bool IsTransparent(int x, int y, byte[,] map, int mapWidth, int mapHeight)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
        {
            return false;
        }

        byte tileValue = map[x, y];
        return tileValue == 0 || tileValue == 3 || tileValue == 1;
    }
}
