using UnityEngine;

public class CombatBalance: MonoBehaviour
{
    //Might be useless if we set hit chances in the cards themselves
    [Header("Base Hit Chance")]
    [Range(0, 100)] public int baseHitChance;
    [Range(0, 100)] public int minHitChance;
    [Range(0, 100)] public int maxHitChance;

    [Header("Distance Penalties")]
    //public int noPenaltyRange = 3;
    public int hitPenaltyPerTile;
    
    [Header("Accuracy Modifiers")] 
    //Multiplies hit chance: 1 = normal, 0.8 = harder, 1.2 = easier
    public float accuracyMultiplier;
    //Flat bonus added to all hit chances after multiplier
    public int accuracyFlatBonus;

    public static CombatBalance instance {get; private set;}

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }
}
