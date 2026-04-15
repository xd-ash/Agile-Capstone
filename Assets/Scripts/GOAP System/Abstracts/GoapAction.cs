using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GOAPEnums;

[System.Serializable]
public abstract class GoapAction
{
    [SerializeField, HideInInspector] protected string actionName;
    public float cost = 1f;
    public bool running = false; //is performing action currently
    [HideInInspector] public GoapAgent agent;

    public Dictionary<string, int> preConditions = new();
    public Dictionary<string, int> postConditions = new();

    public GoapStates preConditionsFlags;
    public GoapStates postConditionsFlags;

    //public GInventory inventory;
    public WorldStates beliefs;

    public GoapAction()
    {
        actionName = this.ToString();
    }

    public void GrabConditionsFromEnums()
    {
        var tempPreCond = GetAllStatesFromFlags(preConditionsFlags);
        var tempPostCond = GetAllStatesFromFlags(postConditionsFlags);

        foreach (var c in tempPreCond)
            if (preConditions == null || !preConditions.ContainsKey(c.key))
                preConditions.Add(c.key, c.value);
        foreach (var c in tempPostCond)
            if (postConditions == null || !postConditions.ContainsKey(c.key))
                postConditions.Add(c.key, c.value);

        List<string> tempPreToString = new List<string>(), 
                     tempPostToString = new List<string>();

        foreach (var s in tempPreCond)
            tempPreToString.Add(s.key);
        foreach (var s in tempPostCond)
            tempPostToString.Add(s.key);

        ManipulateConditionsLists(tempPreCond, tempPreToString, ref preConditions);
        ManipulateConditionsLists(tempPostCond, tempPostToString, ref postConditions);


        //inventory = this.GetComponent<GAgent>().inventory;
        //beliefs = this.GetComponent<GoapAgent>().beliefs;
    }
    private void ManipulateConditionsLists(List<WorldState> stateList, List<string> stringList, ref Dictionary<string, int> conditions)
    {
        for (int i = conditions.Count - 1; i >= 0; i--)
        {
            string key = conditions.ElementAt(i).Key;
            if (key == null)
            {
                conditions.Remove(key);
                continue;
            }

            if (!stringList.Contains(key) || key == string.Empty)
                conditions.Remove(key);
        }

        if (conditions.Count == 0)
            foreach (var s in stateList)
                conditions.Add(s.key, s.value);
        else
        {
            foreach (var s in stateList)
            {
                bool goalPresent = false;

                foreach (var c in conditions)
                    if (s.key == c.Key)
                    {
                        goalPresent = true;
                        break;
                    }

                if (!goalPresent)
                    conditions.Add(s.key, s.value);
            }
        }
    }
    public bool IsAchievable()
    {
        return true;
    }
    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        /*/
        string debugmessagfe = "";
        foreach (var s in conditions)
            debugmessagfe += $"{s.Key}, ";
        Debug.Log($"Conditions Param: " + debugmessagfe);
        string debugqweqwmessagfe = "";
        foreach (var p in preConditions)
            debugqweqwmessagfe += $"{p.Key}, ";
        Debug.Log($"preconds: " + debugqweqwmessagfe);
        /*/

        foreach (KeyValuePair<string, int> kvp in preConditions)
            if (!conditions.ContainsKey(kvp.Key))
                return false;
        return true;
    }
    public abstract bool PrePerform(ref WorldStates beliefs);
    public abstract void Perform();
    public abstract void PostPerform(ref WorldStates beliefs);
}
