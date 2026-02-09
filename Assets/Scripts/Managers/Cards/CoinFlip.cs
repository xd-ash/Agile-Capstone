using UnityEngine;

public class CoinFlip : MonoBehaviour
{
    public bool FlipCoin()
    {
        // Simulate a coin flip: 0 for heads, 1 for tails
        int result = Random.Range(0, 2);
        Debug.Log("Coin flip result: " + (result == 0 ? "Heads" : "Tails"));
        return result == 0; // Return true for heads, false for tails
    }
}
