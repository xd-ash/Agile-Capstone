using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete misc effect class. Performs a coin flip and exposes the result as an output port.
    [CreateNodeMenu("Chance Effect")]
    public class ChanceEffect : MiscEffect
    {
        // Node output: true = heads, false = tails
        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)]
        public bool coinResult;

        // Called by the ability system to start this effect. Sets coinResult and signals completion.
        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            coinResult = FlipCoin();
            Debug.Log($"[ChanceEffect] Coin flip result: {(coinResult ? "Heads" : "Tails")}");
            onFinished?.Invoke();
        }

        // Simple coin flip helper
        private bool FlipCoin()
        {
            // Unity's Random.Range(0,2) returns 0 or 1.
            int result = UnityEngine.Random.Range(0, 2);
            return result == 0; // true for heads, false for tails
        }
    }
}