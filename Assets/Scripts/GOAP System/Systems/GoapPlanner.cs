using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GOAPNode
{
    public GOAPNode parent;
    public float cost;
    public Dictionary<string, int> state;
    public GoapAction action;

    public GOAPNode(GOAPNode parent, float cost, Dictionary<string, int> allStates, GoapAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, int>(allStates);
        this.action = action;
    }
    public GOAPNode(GOAPNode parent, float cost, Dictionary<string, int> allStates, Dictionary<string, int> beliefStates, GoapAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, int>(allStates);
        foreach(KeyValuePair<string, int> b in beliefStates)
            if(!this.state.ContainsKey(b.Key))
                this.state.Add(b.Key, b.Value);
        this.action = action;
    }
}

public class GoapPlanner
{
    public Queue<GoapAction> Plan(List<GoapAction> actions, Dictionary<string,int> goal, WorldStates beliefStates)
    {
        List<GoapAction> usableActions = new List<GoapAction>();
        foreach (GoapAction a in actions)
            if (a.IsAchievable())
                usableActions.Add(a);

        List<GOAPNode> leaves = new List<GOAPNode>();
        GOAPNode start = new GOAPNode(null, 0/*, GoapWorld.Instance.GetWorld().GetStates()*/, beliefStates.GetStates(), null); //null parent, no cost, & null action b/c it is start node

        /*/
        string tempStr = "Actions: ";
        foreach (var a in usableActions)
            tempStr += a.ToString() + ", ";
        tempStr += "\nGoals: ";
        foreach (var g in goal)
            tempStr += g.Key + ", ";
        tempStr += "\nBeliefs: ";
        foreach (var b in beliefStates.GetStates())
            tempStr += b.Key + ", ";
        Debug.Log(tempStr);
        /*/

        bool success = BuildGraph(start, leaves, usableActions, goal);

        if (!success)
        {
            Debug.Log("NO PLAN");
            return null;
        }

        GOAPNode cheapest = null;
        foreach (GOAPNode leaf in leaves)
        {
            if (cheapest == null)
                cheapest = leaf;
            else
                if (leaf.cost < cheapest.cost)
                    cheapest = leaf;
        }

        List<GoapAction> result = new List<GoapAction>();
        GOAPNode n = cheapest;
        while (n != null)
        {
            if(n.action != null)
                result.Insert(0,n.action);
            n = n.parent;
        }

        Queue<GoapAction> queue = new Queue<GoapAction>();
        foreach (GoapAction a in result)
            queue.Enqueue(a);

        /*/
        string tempStr2 = "The Plan is: ";
        foreach (GoapAction a in queue)
            tempStr2 += $"{a.ToString()} > ";
        Debug.Log(tempStr2);
        /*/
        return queue;
    }

    //recursive method for node graph building
    private bool BuildGraph(GOAPNode parent, List<GOAPNode> leaves, List<GoapAction> usableActions, Dictionary<string, int> goal)
    {
        bool foundPath = false;
        foreach (GoapAction action in usableActions)
        {
            if (action.IsAchievableGiven(parent.state))
            {
                Dictionary<string, int> currentState = new Dictionary<string, int>(parent.state);

                //Debug.Log($"Action (post count): {action.ToString()}({action.postConditions.Count})");

                foreach (KeyValuePair<string, int> eff in action.postConditions)
                    if (!currentState.ContainsKey(eff.Key))
                    {
                        //Debug.Log("test curstate contains key");
                        currentState.Add(eff.Key, eff.Value);
                    }

                // No belief param needed as worldstates are concatenated in
                GOAPNode node = new GOAPNode(parent, parent.cost + action.cost, currentState, action); //parent cost + action cost for accumulating costs as plan is created
                if(GoalAchieved(goal, currentState))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    //Debug.Log("starting new recurs");
                    List<GoapAction> subset = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                        foundPath = true;
                }
            }
        }

        return foundPath;
    }
    private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
    {
        foreach (KeyValuePair<string, int> g in goal)
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
