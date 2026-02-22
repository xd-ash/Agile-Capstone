using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GOAPEnums;

[System.Serializable]
public abstract class GoapAction
{
    [SerializeField, HideInInspector] protected string _actionName;
    [SerializeField] protected float _cost = 1f;
    [SerializeField] protected bool _isRunning = false; //is performing action currently
    [HideInInspector] protected GoapAgent agent;

    protected Dictionary<string, int> _preConditions = new();
    protected Dictionary<string, int> _postConditions = new();

    [SerializeField] protected GoapStates _preConditionsFlags;
    [SerializeField] protected GoapStates _postConditionsFlags;

    //public WorldStates beliefs;

    public float GetCost => _cost;
    public bool IsRunning { get { return _isRunning; } set { _isRunning = value; } }
    public Dictionary<string, int> GetPreConditions => _preConditions;
    public Dictionary<string, int> GetPostConditions => _postConditions;

    public GoapAction(GoapAgent agent)
    {
        this.agent = agent;
        _actionName = this.ToString();
    }

    public void GrabConditionsFromEnums()
    {
        var tempPreCond = GetAllStatesFromFlags(_preConditionsFlags);
        var tempPostCond = GetAllStatesFromFlags(_postConditionsFlags);

        foreach (var c in tempPreCond)
            if (_preConditions == null || !_preConditions.ContainsKey(c.key))
                _preConditions.Add(c.key, c.value);
        foreach (var c in tempPostCond)
            if (_postConditions == null || !_postConditions.ContainsKey(c.key))
                _postConditions.Add(c.key, c.value);

        List<string> tempPreToString = new List<string>(), 
                     tempPostToString = new List<string>();

        foreach (var s in tempPreCond)
            tempPreToString.Add(s.key);
        foreach (var s in tempPostCond)
            tempPostToString.Add(s.key);

        ManipulateConditionsLists(tempPreCond, tempPreToString, ref _preConditions);
        ManipulateConditionsLists(tempPostCond, tempPostToString, ref _postConditions);

        //inventory = this.GetComponent<GAgent>().inventory;
        //beliefs = this.GetComponent<GoapAgent>().beliefs;
    }
    protected void ManipulateConditionsLists(List<WorldState> stateList, List<string> stringList, ref Dictionary<string, int> conditions)
    {
        if (conditions == null) conditions = new();

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

        foreach (KeyValuePair<string, int> kvp in _preConditions)
            if (!conditions.ContainsKey(kvp.Key))
                return false;
        return true;
    }
    public abstract bool PrePerform(ref WorldStates beliefs);
    public abstract void Perform();
    public abstract void PostPerform(ref WorldStates beliefs);
}
