using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour
{
    //public string actionName;
    public float cost = 1f;
    public bool running = false; //is performing action currently

    // arrays here for easy inspector condition editing
    [SerializeField] private WorldState[] _preConditionsArray;
    [SerializeField] private WorldState[] _postConditionsArray;

    public Dictionary<GoapStates, int> preConditions;
    public Dictionary<GoapStates, int> postConditions;

    //public GInventory inventory;
    public WorldStates beliefs;

    public GoapAction()
    {
        preConditions = new Dictionary<GoapStates, int>();
        postConditions = new Dictionary<GoapStates, int>();
    }
    private void Awake()
    {
        if (_preConditionsArray != null )
            foreach (WorldState w in _preConditionsArray)
                preConditions.Add(w.key, w.value);

        if (_postConditionsArray != null )
            foreach (WorldState w in _postConditionsArray)
                postConditions.Add(w.key, w.value);

        //inventory = this.GetComponent<GAgent>().inventory;
        beliefs = this.GetComponent<GoapAgent>().beliefs;
    }
    public bool IsAchievable()
    {
        return true;
    }
    public bool IsAchievableGiven(Dictionary<GoapStates, int> conditions)
    {
        foreach (KeyValuePair<GoapStates, int> kvp in preConditions)
            if (!conditions.ContainsKey(kvp.Key))
                return false;
        return true;
    }

    public abstract bool PrePerform();
    public abstract bool PostPerform();
}
