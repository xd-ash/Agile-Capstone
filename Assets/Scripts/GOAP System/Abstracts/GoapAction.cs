using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class GoapAction
{
    //public string actionName;
    public float cost = 1f;
    public bool running = false; //is performing action currently

    // arrays here for easy inspector condition editing
    //[SerializeField] private WorldState[] _preConditionsArray;
    //[SerializeField] private WorldState[] _postConditionsArray;

    public Dictionary<string, int> preConditions;
    public Dictionary<string, int> postConditions;

    public GoapStates preConditionsFlags;
    public GoapStates postConditionsFlags;

    //public GInventory inventory;
    public WorldStates beliefs;

    public GoapAction()
    {
        preConditions = new Dictionary<string, int>();
        postConditions = new Dictionary<string, int>();
    }
    /*
    private void Awake()
    {

        /*
        if (_preConditionsArray != null )
            foreach (WorldState w in _preConditionsArray)
                preConditions.Add(w.key, w.value);

        if (_postConditionsArray != null )
            foreach (WorldState w in _postConditionsArray)
                postConditions.Add(w.key, w.value);
        
        //inventory = this.GetComponent<GAgent>().inventory;
        beliefs = this.GetComponent<GoapAgent>().beliefs;
    }*/
    public bool IsAchievable()
    {
        return true;
    }
    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach (KeyValuePair<string, int> kvp in preConditions)
            if (!conditions.ContainsKey(kvp.Key))
                return false;
        return true;
    }
    public abstract bool PostPerform();
    public abstract bool PrePerform();
}
