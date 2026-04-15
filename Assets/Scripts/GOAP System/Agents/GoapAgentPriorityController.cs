using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class GoapAgentPriorityController : MonoBehaviour
{
    //public enum AgentPrioState { KillPlayer, StayAlive, HelpAllies }
    //private AgentPrioState _agentPrioState;
    
    private GoapAgent _agent;
    private Unit _unit;
    private GoapAgentSO _so;

    private void Awake()
    {
        if (TryGetComponent(out _agent))
        {
            _unit = _agent.unit;
            _so = _agent.GetAgentSO;
        }
    }

    public float CalculateGoalPriority(string goalName, float initialPrio)
    {
        if (goalName == GoapGoals.KillPlayer.ToString())
            return CalculateKillPlayerPrio(initialPrio);
        else if (goalName == GoapGoals.StayAlive.ToString())
            return CalculateStayAlivePrio(initialPrio);
        else if (goalName == GoapGoals.KeepAlliesAlive.ToString())
            return CalculateKeepAlliesAlivePrio(initialPrio);
        else //catch for end turn goal
            return -1;
    }

    private float CalculateKillPlayerPrio(float initialPrio)
    {
        if (_agent == null || _agent.GetCurrentTarget == null) return 0;
        
        return -1f;
    }
    private float CalculateStayAlivePrio(float initialPrio)
    {
        return -1f;
    }
    private float CalculateKeepAlliesAlivePrio(float initialPrio)
    {
        return -1f;
    }
    
    private float CalcWeights(float[] values, float[] maxes)
    {
        if (values.Length != maxes.Length) return 0;
        return values.Sum() / maxes.Sum();
    }
}
