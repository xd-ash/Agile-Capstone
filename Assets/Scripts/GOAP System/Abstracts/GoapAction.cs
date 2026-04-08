using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GOAPEnums;

[System.Serializable]
public abstract class GoapAction
{
    [SerializeField, HideInInspector] protected string _actionName;

    [SerializeField] protected float _cost = 1f;
    [SerializeField] protected float _costMultiplier = 1f;
    protected bool _isRunning = false; //is performing action currently
    protected GoapAgent _agent;

    protected Dictionary<string, float> _preConditions = new();
    protected Dictionary<string, float> _postConditions = new();

    [SerializeField] protected GoapStates _preConditionsFlags;
    [SerializeField] protected GoapStates _postConditionsFlags;
    [SerializeField] protected GoapGoals _goalsFlags;

    public string GetActionName => _actionName;
    public bool IsRunning { get { return _isRunning; } set { _isRunning = value; } }
    public Dictionary<string, float> GetPreConditions => _preConditions;
    public Dictionary<string, float> GetPostConditions => _postConditions;
    public GoapAction(string overrideName = "")
    {
        _actionName = overrideName == string.Empty ? this.ToString() : overrideName;
    }

    public void SetAgent(GoapAgent agent)
    {
        _agent = agent;
    }
    public void GrabConditionsFromEnums()
    {
        var tempPreCond = GetAllStatesFromFlags(_preConditionsFlags);
        var tempPostCond = GetAllStatesFromFlags(_postConditionsFlags, _goalsFlags);

        foreach (var c in tempPreCond)
            if (_preConditions != null && !_preConditions.ContainsKey(c.key))
                _preConditions.Add(c.key, c.value);
        foreach (var c in tempPostCond)
            if (_postConditions != null && !_postConditions.ContainsKey(c.key))
                _postConditions.Add(c.key, c.value);
        
        List<string> tempPreToString = new List<string>(), 
                     tempPostToString = new List<string>();

        foreach (var s in tempPreCond)
            tempPreToString.Add(s.key);
        foreach (var s in tempPostCond)
            tempPostToString.Add(s.key);

        ManipulateConditionsLists(tempPreCond, tempPreToString, ref _preConditions);
        ManipulateConditionsLists(tempPostCond, tempPostToString, ref _postConditions);
    }
    protected void ManipulateConditionsLists(List<WorldState> stateList, List<string> stringList, ref Dictionary<string, float> conditions)
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
    public bool IsAchievableGiven(Dictionary<string, float> conditions, bool  showDebug = false)
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

        foreach (KeyValuePair<string, float> kvp in _preConditions)
            if (!conditions.ContainsKey(kvp.Key))
            {
                //if (showDebug)
                    //Debug.Log($"{this} is not achievable given {string.Join(", ", conditions.Keys)}");
                return false;
            }
        //if (showDebug)
            //Debug.Log($"{this} is achievable given {string.Join(", ", conditions.Keys)}");
        return true;
    }
    public abstract bool PrePerform(ref WorldStates beliefs);
    public abstract void Perform();
    public abstract void PostPerform(ref WorldStates beliefs);
    public abstract float EvaluateCost(Unit tempTarget = null);
}
