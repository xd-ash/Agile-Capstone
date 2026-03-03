using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialDiceOutcome
{
    None,
    SnakeEyes,
    Double,
}
public static class DiceRoll
{
    public static Action<Unit, int> DiceRolled;

    public static int[] RollDice(Unit unit, int numRolls = 1)
    {
        List<int> tempDiceResults = new();

        for (int i = 0; i < numRolls; i++)
        {
            int result = RollDie(unit);
            tempDiceResults.Add(result);
            DiceRolled?.Invoke(unit, result);
        }

        return tempDiceResults.ToArray();
    }

    public static bool RollDice(Unit unit, int[] desiredOutcomes, int numRolls = 1)
    {
        List<int> tempDiceResults = new();

        for (int i = 0; i < numRolls; i++)
        {
            int result = RollDie(unit);
            tempDiceResults.Add(result);
        }

        foreach (int outcome in desiredOutcomes)
            if (tempDiceResults.Contains(outcome))
                return true;
        return false;
    }

    public static List<Tuple<int, int>> RollDicePair(Unit unit, int numRolls = 1)
    {
        List<Tuple<int, int>> tempDiceResults = new();

        for (int i = 0; i < numRolls; i++)
        {
            int result1 = RollDie(unit);
            int result2 = RollDie(unit);
            tempDiceResults.Add(new(result1,result2));
        }

        return tempDiceResults;
    }
    public static SpecialDiceOutcome RollDicePairForDoubles(Unit unit, int numRolls = 1)
    {
        List<Tuple<int, int>> tempDiceResults = new();

        for (int i = 0; i < numRolls; i++)
        {
            int result1 = RollDie(unit);
            int result2 = RollDie(unit);
            tempDiceResults.Add(new(result1,result2));
        }
        foreach (var result in tempDiceResults)
        {
            if (result.Item1 != result.Item2) continue;
            if (result.Item1 == 1)
                return SpecialDiceOutcome.SnakeEyes;
            else
                return SpecialDiceOutcome.Double;
        }
        return SpecialDiceOutcome.None;
    }

    private static int RollDie(Unit unit)
    {
        int result = UnityEngine.Random.Range(1, 7);
        DiceRolled?.Invoke(unit, result);
        return result;
    }
}
