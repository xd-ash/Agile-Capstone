using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GOAPNode
{
    public GOAPNode parent;
    public float cost;
    public Dictionary<string, float> state;
    public GoapAction action;

    public GOAPNode(GOAPNode parent, float cost, Dictionary<string, float> allStates, GoapAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, float>(allStates);
        this.action = action;
    }
    public GOAPNode(GOAPNode parent, float cost, Dictionary<string, float> allStates, Dictionary<string, float> beliefStates, GoapAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, float>(allStates);
        foreach(KeyValuePair<string, float> b in beliefStates)
            if(!this.state.ContainsKey(b.Key))
                this.state.Add(b.Key, b.Value);
        this.action = action;
    }
}

public class Plan
{
    public float cheapestCost;
    public Queue<GoapAction> actionQueue;
    public Unit[] targets;
}

public class GoapPlanner
{
    // temp solution for grabbing a debug bool
    public GoapPlanner(GoapAgent agent)
    {
        _agent = agent;
    }
    private GoapAgent _agent;
    //

    //public Tuple<float, Queue<GoapAction>> Plan(List<GoapAction> actions, Dictionary<string, float> goal, WorldStates beliefStates)
    public Plan Plan(List<GoapAction> actions, Dictionary<string, float> goal, WorldStates beliefStates)
    {
        List<GoapAction> usableActions = new List<GoapAction>();
        foreach (GoapAction a in actions)
            if (a.IsAchievable())
                usableActions.Add(a);

        //setup temp beliefs for planning
        Unit tempTarget = null;
        string curGoal = goal.ElementAt(0).Key;
        int i = curGoal == GoapGoals.KeepAlliesAlive.ToString() || curGoal == GoapGoals.StayAlive.ToString() ? 1 : 0;
        var targets = ChooseTargetAction.GetCurrentTargets(curGoal, _agent);
        tempTarget = targets[i];

        var tempBeliefs = _agent.GetTempBeliefsGivenGoal(curGoal, tempTarget, beliefStates);

        List<GOAPNode> leaves = new();
        //GOAPNode start = new GOAPNode(null, 0, beliefStates.GetStates, null); //null parent, no cost, & null action b/c it is start node
        GOAPNode start = new GOAPNode(null, 0, tempBeliefs.GetStates, null); //null parent, no cost, & null action b/c it is start node

        bool success = BuildGraph(start, leaves, usableActions, goal, tempTarget);

        //
        if (_agent.showDebugMessages)
        {
            string tempStr = $"Agent: {_agent.name} Target: {tempTarget.name} - Goal: ";
            foreach (var g in goal)
                tempStr += g.Key + ", ";
            tempStr += $"\ntempBeliefs: ";
            foreach (var b in tempBeliefs.GetStates)
            //foreach (var b in beliefStates.GetStates)
                tempStr += b.Key + ", ";
            Debug.Log(tempStr);
        }
        //

        if (!success)
        {
            if(_agent.showDebugMessages)
                Debug.Log("Graph Build Fail - No Plan");
            return null;
        }

        // find cheapest path 
        GOAPNode cheapest = null;
        foreach (GOAPNode leaf in leaves)
        {
            if (cheapest == null)
                cheapest = leaf;
            else
                if (leaf.cost < cheapest.cost)
                    cheapest = leaf;
        }

        // cycle through each parent in order to add all actions
        List<GoapAction> result = new List<GoapAction>();
        GOAPNode n = cheapest;
        while (n != null)
        {
            if(n.action != null)
                result.Insert(0, n.action);
            n = n.parent;
        }

        // create action queue from cheapest list of actions
        Queue<GoapAction> queue = new Queue<GoapAction>();
        foreach (GoapAction a in result)
            queue.Enqueue(a);

        //
        if (_agent.showDebugMessages)
        {
            string tempStr2 = $"{_agent.name} - Possible Plan: ";
            foreach (GoapAction a in queue)
                tempStr2 += $"{a.GetActionName} > ";
            //string target = _agent.GetCurrentTarget == null ? "null" : _agent.GetCurrentTarget.name;
            Debug.Log(tempStr2 + $"(Cost: {cheapest.cost}, Target:{tempTarget?.name}, Goal: {goal?.ElementAt(0).Key})");
        }
        //

        var plan = new Plan() { cheapestCost = cheapest.cost, actionQueue = queue, targets = targets };
        return plan;
    }

    //recursive method for node graph building 
    private bool BuildGraph(GOAPNode parent, List<GOAPNode> leaves, List<GoapAction> usableActions, Dictionary<string, float> goal, Unit tempTarget)
    {
        string curGoal = goal.ElementAt(0).Key;
        bool foundPath = false;
        foreach (GoapAction action in usableActions)
        {
            if (!action.IsAchievableGiven(parent.state)) continue;
            
            Dictionary<string, float> currentState = new Dictionary<string, float>(parent.state);

            foreach (KeyValuePair<string, float> eff in action.GetPostConditions)
                if (!currentState.ContainsKey(eff.Key))
                    currentState.Add(eff.Key, eff.Value);

            // No belief param needed as worldstates are concatenated in
            GOAPNode node = new GOAPNode(parent, parent.cost + action.EvaluateCost(curGoal, tempTarget), currentState, action); //parent cost + action cost for accumulating costs as plan is created
            if(GoalAchieved(goal, currentState))
            {
                leaves.Add(node);
                foundPath = true;
            }
            else
            {
                List<GoapAction> subset = ActionSubset(usableActions, action);
                foundPath = BuildGraph(node, leaves, subset, goal, tempTarget); // at this point build graph from subset. On success, bool follows stack back to first call
            }
        }

        return foundPath;
    }
    private bool GoalAchieved(Dictionary<string, float> goal, Dictionary<string, float> state)
    {
        foreach (KeyValuePair<string, float> g in goal)
            if (!state.ContainsKey(g.Key))
                return false;

        return true;
    }

    //build new list w/o removeMe action
    private List<GoapAction> ActionSubset(List<GoapAction> actions, GoapAction removeMe)
    {
        List<GoapAction> subset = new List<GoapAction>();
        foreach (GoapAction a in actions)
            if (!a.Equals(removeMe))
                subset.Add(a);

        return subset;
    }
}
