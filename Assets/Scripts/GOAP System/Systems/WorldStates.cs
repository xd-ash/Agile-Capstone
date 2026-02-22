using System.Collections.Generic;

// small data class to store dictionary key & value.
// Serializable to easily manipulate in inspector
[System.Serializable]
public class WorldState
{
    public string key;
    public int value;
}

public class WorldStates
{
    private Dictionary<string, int> _states;
    public Dictionary<string, int> GetStates => _states;

    public WorldStates()
    {
        _states = new Dictionary<string, int>();
    }
    public bool HasState(string key)
    {
        return _states.ContainsKey(key);
    }
    void AddState(string key, int value)
    {
        _states.Add(key, value);
    }
    public void ModifyState(string key, int value)
    {
        if (_states.ContainsKey(key))
        {
            _states[key] += value;
            if (_states[key] <= 0) // only use if don't want negative values in worldstate obj
                RemoveState(key);
        }
        else
            _states.Add(key, value);
    }
    public void RemoveState(string key)
    {
        if (_states.ContainsKey(key))
            _states.Remove(key);
    }
    public void SetState(string key, int value)
    {
        if (_states.ContainsKey(key))
            _states[key] = value;
        else
            _states.Add(key, value);
    }
}
