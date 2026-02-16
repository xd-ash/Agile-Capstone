using System;
using System.Collections.Generic;
using UnityEngine;

public static class CoinFlip
{
    public static Action<Unit, bool> CoinFlipped;

    public static bool[] FlipCoin(Unit unit, int numCoinFlips = 1)
    {
        List<bool> tempCoinFlips = new();

        for (int i = 0; i < numCoinFlips; i++)
        {
            UnityEngine.Random.InitState(DateTime.Now.Millisecond);
            bool result = UnityEngine.Random.Range(0, 2) == 1 ? true : false;// Heads(1) - True, Tails(0) - False
            tempCoinFlips.Add(result);
            CoinFlipped?.Invoke(unit, result);
        }
        
        return tempCoinFlips.ToArray();
    }

    public static bool[] FlipCoin(Unit unit, bool desiredOutcome, int maxFlips)
    {
        List<bool> tempCoinFlips = new();
        
        do
        {
            UnityEngine.Random.InitState(DateTime.Now.Millisecond);
            bool result = UnityEngine.Random.Range(0, 2) == 1 ? true : false;// Heads(1) - True, Tails(0) - False
            tempCoinFlips.Add(result);
            CoinFlipped?.Invoke(unit, result);
        } while (!tempCoinFlips.Contains(desiredOutcome) && tempCoinFlips.Count < maxFlips);

        return tempCoinFlips.ToArray();
    }
}
