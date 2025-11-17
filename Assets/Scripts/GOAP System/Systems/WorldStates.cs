using System;
using System.Collections.Generic;
using UnityEngine;

// small data class to store future dictionary key & value.
// Serializable to easily manipulate in inspector
[System.Serializable]
public class WorldState
{
    public GoapStates key;
    private int value;
}

public class WorldStates
{
    public Dictionary<GoapStates, int> states;

    public WorldStates()
    {
        states = new Dictionary<GoapStates, int>();
    }
    public bool HasState(GoapStates key)
    {
        return states.ContainsKey(key);
    }
    void AddState(GoapStates key, int value)
    {
        states.Add(key, value);
    }
    public void ModifyState(GoapStates key, int value)
    {
        if (states.ContainsKey(key))
        {
            states[key] += value;
            if (states[key] <= 0) // only use if don't want negative values in worldstate obj
                RemoveState(key);
        }
        else
            states.Add(key, value);
    }
    public void RemoveState(GoapStates key)
    {
        if (states.ContainsKey(key))
            states.Remove(key);
    }
    public void SetState(GoapStates key, int value)
    {
        if (states.ContainsKey(key))
            states[key] = value;
        else
            states.Add(key, value);
    }
    public Dictionary<GoapStates, int> GetStates()
    {
        return states;
    }
}
