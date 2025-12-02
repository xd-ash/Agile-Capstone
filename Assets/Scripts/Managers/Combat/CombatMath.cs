using UnityEngine;
using AStarPathfinding;
using static IsoMetricConversions;

public static class CombatMath
{
    //Default fallback numbers if no CombatBalance in the scene
    private const int _defaultHitChance = 80;
    private const int _defaultMinHitChance = 10;
    private const int _defaultMaxHitChance = 95;
    private const int _defaultNoPenaltyRange = 3;
    private const int _defaultPenaltyPerTile = 5;
    private const float _defaultGlobalMultiplier = 1f;
    private const int _defaultGlobalFlatBonus = 0;
    
    private static CombatBalance CB => CombatBalance.instance;
    
    //Don't know if this is the proper way of doing things
    private static int BaseHitChance => CB != null ? CB.baseHitChance : _defaultHitChance;
    private static int MinHitChance => CB != null ? CB.minHitChance : _defaultMinHitChance;
    private static int MaxHitChance => CB != null ? CB.maxHitChance : _defaultMaxHitChance;
    //private static int NoPenaltyRange => CB != null ? CB.noPenaltyRange : _defaultNoPenaltyRange;
    private static int PenaltyPerTile => CB != null ? CB.hitPenaltyPerTile : _defaultPenaltyPerTile;
    private static float AccuracyMultiplier => CB != null ? CB.accuracyMultiplier : _defaultGlobalMultiplier;
    private static int AccuracyFlatBonus => CB != null ? CB.accuracyFlatBonus : _defaultGlobalFlatBonus;


    //Returns the hit chance (0–100) from attacker to target.
    //Returns 0 if line of sight is blocked or if either unit is null.
    public static int GetHitChance(Unit attacker, Unit target, int abilityRange)
    {
        if (attacker == null || target == null)
        {
            return 0;
        }

        if (!HasLineOfSight(attacker, target))
        {
            return 0;
        }

        Vector2Int attackerCell = ConvertToGridFromIsometric(attacker.transform.localPosition);
        Vector2Int targetCell = ConvertToGridFromIsometric(target.transform.localPosition);

        int distance = Mathf.Abs(attackerCell.x - targetCell.x) + Mathf.Abs(attackerCell.y - targetCell.y); //Manhattan distance

        int extraDistance = Mathf.Max(0, distance - abilityRange);
        int distancePenalty;

        //added logic here for melee abilties to not be used outside of melee range
        if (abilityRange > 1)
            distancePenalty = extraDistance * PenaltyPerTile;
        else
            distancePenalty = BaseHitChance;
        int rawHitChance = BaseHitChance - distancePenalty;
        
        float scaled = rawHitChance * AccuracyMultiplier;
        int final = Mathf.RoundToInt(scaled) + AccuracyFlatBonus;
        
        final = Mathf.Clamp(final, MinHitChance, MaxHitChance);

        return final;
    }
    
    //Rolls a random number against the current hit chance.
    public static bool RollHit(Unit attacker, Unit target, int abilityRange, out int hitChance, out float roll)
    {
        hitChance = GetHitChance(attacker, target, abilityRange);

        if (hitChance <= 0)
        {
            roll = 100f;
            return false;
        }

        roll = Random.Range(0f, 100f);
        return roll <= hitChance;
    }

    public static bool HasLineOfSight(Unit attacker, Unit target)
    {
        if (attacker == null || target == null)
            return false;

        if (MapCreator.instance == null)
            return true; //Fail open instead of breaking combat.

        Vector2Int attackerCell = ConvertToGridFromIsometric(attacker.transform.localPosition);
        Vector2Int targetCell = ConvertToGridFromIsometric(target.transform.localPosition);

        return HasLineOfSight(attackerCell, targetCell);
    }
    
    //Line of sight check on the byte map using a bresenham style grid line.
    //Returns true if every tile between start and end is transparent.
    public static bool HasLineOfSight(Vector2Int startCell, Vector2Int endCell)
    {
        byte[,] map = MapCreator.instance.GetByteMap;
        if (map == null) return true;

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

            //Only tiles between the units can block LOS.
            if (!isStartCell && !isEndCell)
            {
                if (!IsTransparent(currentX, currentY, map, mapWidth, mapHeight))
                    return false;
            }

            if (isEndCell)
                break;

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

    //Determines whether a given tile lets line of sight pass through.
    private static bool IsTransparent(int x, int y, byte[,] map, int mapWidth, int mapHeight)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
        {
            return false;
        }

        byte tileValue = map[x, y];

        //0 = empty - transparent
        //1 = obstacle - blocks LOS
        //2 = enemy spawn/marker - transparent
        return tileValue == 0 || tileValue == 2;
    }
}
