using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

public class Goal
{
    public Dictionary<string, int> sGoals;
    public bool remove; //removes goal once completed

    public Goal(string s, int i, bool r)
    {
        sGoals = new Dictionary<string, int>();
        sGoals.Add(s, i);
        remove = r;
    }
}

public abstract class GoapAgent : MonoBehaviour
{
    [SerializeField] private GoapActions _goapActionsEnum;
    [SerializeReference] public List<GoapAction> actions = new();
    public Dictionary<Goal, int> goals = new Dictionary<Goal, int>();
    //public GoapInventory inventory = new GoapInventory();
    public WorldStates beliefs = new WorldStates();

    GoapPlanner planner;
    Queue<GoapAction> actionQueue;
    public GoapAction currentAction;
    Goal currentGoal;

    protected virtual void Start()
    {
        GoapAction[] acts = this.GetComponents<GoapAction>();
        foreach (GoapAction a in acts)
            actions.Add(a);
    }
    public void GrabActionsFromEnum()
    {
        var temp = GOAPEnums.GetAllTypesFromFlags(_goapActionsEnum);

        List<string> tempToString = new List<string>(),
                     actionsToString = new List<string>();
        foreach (var a in temp)
            tempToString.Add(a.ToString());
        if (actions.Count > 0)
            foreach (var a in actions)
                actionsToString.Add(a.ToString());

        for (int i = 0; i < actionsToString.Count; i++)
        {
            if (actionsToString[i] == null)
            {
                actions.RemoveAt(i);
                continue;
            }

            if (!tempToString.Contains(actionsToString[i]))
                actions.RemoveAt(i);
        }

        if (actions.Count == 0)
            foreach (var a in temp)
                actions.Add(a);
        else
        {
            foreach (var a in temp)
            {
                bool actionPresent = false;

                foreach (var b in actions)
                    if (a.ToString() == b.ToString())
                    {
                        actionPresent = true;
                        break;
                    }

                if (!actionPresent)
                    actions.Add(a);
            }
        }
    }

    //bool invoked = false;

    void CompleteAction()
    {
        currentAction.running = false;
        currentAction.PostPerform();
        //invoked = false;
    }

    void LateUpdate()
    {
        /* Navmesh from original
        if (currentAction != null && currentAction.running)
        {
            float distanceToTarget = Vector3.Distance(currentAction.target.transform.position, this.transform.position);
            if (currentAction.agent.hasPath && distanceToTarget < 2f)
            {
                if (!invoked)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    invoked = true;
                }
            }
            return;
        }
        */
        if (planner == null || actionQueue == null)
        {
            planner = new GoapPlanner();

            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            foreach (KeyValuePair<Goal, int> sg in sortedGoals)
            {
                actionQueue = planner.Plan(actions, sg.Key.sGoals, beliefs); 
                if (actionQueue != null)
                {
                    currentGoal = sg.Key;
                    break;
                }
            }
        }

        if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal.remove)
                goals.Remove(currentGoal);
            planner = null;
        }

        /* (Important!)- target/tag based queue stuff 
        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            if (currentAction.PrePerform())
            {
               if (currentAction.target == null && currentAction.targetTag != "")
                    currentAction.target =  GameObject.FindWithTag(currentAction.targetTag);

                if (currentAction.target != null)
                {
                    currentAction.running = true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position); //set destination for navmesh agent
                }
            }
            else
            {
                actionQueue = null;
            }
        }
        */
    }
}
[CustomEditor(typeof(GoapAgent), true)]
public class GOAPAgentEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        GoapAgent agent = (GoapAgent)target;
        agent.GrabActionsFromEnum();

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
