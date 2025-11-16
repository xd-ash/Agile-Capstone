using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionBase
{
    [HideInInspector] public string actionName = "";

    public float cost = 1f;
    //public GameObject target; //where action takes place
    //public string targetTag;
    //public float duration = 0f;

    //public WorldState[] preConditionsArray;
    //public WorldState[] afterEffectsArray;
    public string[] preConditionsArray;
    public string[] afterEffectsArray;

    ///public NavMeshAgent agent;

    public Dictionary<string, int> preConditionsDict = new Dictionary<string, int>();
    public Dictionary<string, int> afterEffectsDict = new Dictionary<string, int>();

    //public GInventory inventory;
    //public WorldStates beliefs;

    public bool running = false; //is performing action currently

    public void SetConditionDicts()
    {
        /*
        if (preConditionsArray != null)
            foreach (WorldState w in preConditionsArray)
                preConditionsDict.Add(w.key, w.value);

        if (afterEffectsArray != null)
            foreach (WorldState w in afterEffectsArray)
                afterEffectsDict.Add(w.key, w.value);

        inventory = this.GetComponent<GAgent>().inventory;
        beliefs = this.GetComponent<GAgent>().beliefs;
        */
    }
    public bool IsAchievable()
    {
        return true;
    }
    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach (KeyValuePair<string, int> kvp in preConditionsDict)
            if (!conditions.ContainsKey(kvp.Key))
                return false;
        return true;
    }
}
