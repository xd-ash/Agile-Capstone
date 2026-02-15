using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public interface ICoinFlip
    {
        public virtual bool[] FlipCoin(int numCoinFlips = 1)
        {
            List<bool> tempCoinFlips = new();

            for (int i = 0; i < numCoinFlips; i++)
            {
                UnityEngine.Random.InitState(DateTime.Now.Millisecond);
                int rng = UnityEngine.Random.Range(0, 2);
                tempCoinFlips.Add(rng == 1 ? true : false); // Heads(1) - True, Tails(0) - False
            }

            return tempCoinFlips.ToArray();
        }

        public virtual bool[] FlipCoin(bool desiredOutcome)
        {
            List<bool> tempCoinFlips = new();

            do
            {
                UnityEngine.Random.InitState(DateTime.Now.Millisecond);
                int rng = UnityEngine.Random.Range(0, 2);
                Debug.Log("test");
                tempCoinFlips.Add(rng == 1 ? true : false); // Heads(1) - True, Tails(0) - False
            } while (!tempCoinFlips.Contains(desiredOutcome));

            return tempCoinFlips.ToArray();
        }
    }
}