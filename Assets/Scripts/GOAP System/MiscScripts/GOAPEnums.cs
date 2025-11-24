using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum GoapActions
{
    None = 0,
    //All = -1,
    MoveInRange = 2,
    Attack = 4,
    Heal = 8,
    ChooseTarget = 16,
}

[Flags]
public enum GoapStates
{
    None = 0,
    //All = -1,
    HasAP = 2,
    IsHurt = 4,
    InRange = 8,
    IsHealthy = 16,
    OutOfRange = 32,
    OutOfAP = 64,
    AttackPlayer = 128,
    HealSelf = 256,
    HasAttacked = 512,
    HasHealed = 1024,
    HasTarget = 2048,
}

public struct GOAPEnums
{
    // Create and return a list of all goap actions determined by the given enum flag.
    public static List<GoapAction> GetAllActionsFromFlags(GoapAgent agent, GoapActions actionsEnum)
    {
        List<GoapAction> actions = new List<GoapAction>();

        // Convert enum flag to binary.
        string binaryEnum = Convert.ToString((int)actionsEnum, 2).PadLeft(8, '0');

        // Loop through each character in the binaryEnum string and add relevant
        // GOAP Actions to the list.
        for (int i = 0; i < binaryEnum.Length; i++)
        {
            switch (i)
            {
                case 0://not implemented
                    break;
                case 1://not implemented
                    break;
                case 2://not implemented
                    break;
                case 3://Choose Target
                    if (binaryEnum[i] == '1')
                        actions.Add(new ChooseTargetAction() { agent = agent });
                    break;
                case 4://Heal
                    if (binaryEnum[i] == '1')
                        actions.Add(new HealAction() { agent = agent });
                    break;
                case 5://Attack
                    if (binaryEnum[i] == '1')
                        actions.Add(new AttackAction() { agent = agent });
                    break;
                case 6://Move
                    if (binaryEnum[i] == '1')
                        actions.Add(new MoveInRangeAction() { agent = agent });
                    break;
                case 7://None
                    break;
            }
        }
        return actions;
    }
    public static List<WorldState> GetAllStatesFromFlags(GoapStates statesEnum)
    {
        List<WorldState> states = new List<WorldState>();

        string[] enumStrings =  statesEnum.ToString().Split(", ");

        foreach (string s in enumStrings)
        {
            if (s != null && s != "None")
            states.Add(new WorldState() { key = s });
        }
        return states;
    }
}