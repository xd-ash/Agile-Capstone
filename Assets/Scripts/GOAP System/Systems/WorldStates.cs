using System.Collections.Generic;

// small data class to store dictionary key & value.
// Serializable to easily manipulate in inspector
[System.Serializable]
public class WorldState
{
    public string key;
    public float value;
}

public class WorldStates
{
    private Dictionary<string, float> _states;
    public Dictionary<string, float> GetStates => _states;

    public WorldStates()
    {
        _states = new Dictionary<string, float>();
    }
    public bool HasState(string key)
    {
        return _states.ContainsKey(key);
    }
    void AddState(string key, float value)
    {
        _states.Add(key, value);
    }
    public void ModifyState(string key, float value)
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
    public void SetState(string key, float value)
    {
        if (_states.ContainsKey(key))
            _states[key] = value;
        else
            _states.Add(key, value);
    }
}
