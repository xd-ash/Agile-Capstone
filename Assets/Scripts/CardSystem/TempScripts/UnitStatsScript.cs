using CardSystem;
using UnityEngine;

public class UnitStatsScript : MonoBehaviour
{
    /**
     * 
     * TEMP SCRIPT 
     * 
     */

    public void TakeDamage(int damage, DamageTypes type)
    {
        Debug.Log($"{damage}{(type == DamageTypes.None ? "" : " " + type.ToString())} damage dealt to {gameObject.name}");
    }
}
