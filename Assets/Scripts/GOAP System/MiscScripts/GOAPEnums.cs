using System;
using System.Collections.Generic;
using System.Diagnostics;

[Flags]
public enum GoapActions
{
    None = 0,
    //All = -1,
    MoveInRange = 2,
    Attack = 4,
    Heal = 8,
    ChooseTarget = 16,
    EndTurn = 32,
    MoveIntoLOS = 64,
    Hide = 128,
    MoveOutOfLOS = 256,
    MoveToRange = 512,
}

[Flags]
public enum GoapStates
{
    None = 0,
    //All = -1,
    HasAP = 2,
    OutOfAP = 4,
    IsHealthy = 8,
    IsHurt = 16,
    InRange = 32,
    OutOfRange = 64,
    CanAttack = 128,
    CanHeal = 256,
    HasLOS = 512,
    NoLOS = 1024,
    HasTarget = 2048,
    NoTarget = 4096,
    AtRange = 8192,
    AtMelee = 16384,
}
[Flags]
public enum GoapGoals
{
    None = 0,
    KillPlayer = 2,
    StayAlive = 4,
    KeepAlliesAlive = 8,
    EndTurn = 16
}

public struct GOAPEnums
{
    // Create and return a list of all goap actions determined by the given enum flag.
    public static List<GoapAction> GetAllActionsFromFlags(GoapActions actionsEnum)
    {
        List<GoapAction> actions = new List<GoapAction>();

        int enumCount = typeof(GoapActions).GetEnumNames().Length;
        // Convert enum flag to binary.
        string binaryEnum = Convert.ToString((int)actionsEnum, 2).PadLeft(enumCount, '0');

        // Loop through each character in the binaryEnum string and add relevant
        // GOAP Actions to the list.
        for (int i = binaryEnum.Length - 1; i >= 0; i--)
        {
            int index = binaryEnum.Length - 1 - i;
            if (binaryEnum[index] == '0') continue;
            switch (i)
            {
                case 0://None
                    break;
                case 1://Move
                    actions.Add(new MoveInRangeAction());
                    break;
                case 2://Attack
                    actions.Add(new AttackAction());
                    break;
                case 3://Heal
                    actions.Add(new HealAction());
                    break;
                case 4://Choose Target
                    actions.Add(new ChooseTargetAction());
                    break;
                case 5://EndTurn
                    actions.Add(new EndTurnAction());
                    break;
                case 6://MoveIntoLOS
                    actions.Add(new MoveIntoLOSAction());
                    break;
                case 7://Hide
                    actions.Add(new HideAction());
                    break;
                case 8://MoveOutOfLOS
                    actions.Add(new MoveOutOfLOSAction());
                    break;
                case 9://MoveToRange
                    actions.Add(new MoveToRangeAction());
                    break;
            }
        }
        return actions;
    }
    public static List<WorldState> GetAllStatesFromFlags(GoapStates statesEnum, GoapGoals goalsEnum)
    {
        List<WorldState> states = new List<WorldState>();

        string[] statesStrings = statesEnum.ToString().Split(", ");
        string[] goalsStrings = goalsEnum.ToString().Split(", ");

        if ((int)statesEnum == -1)
            statesStrings = typeof(GoapStates).GetEnumNames();
        if ((int)goalsEnum == -1)
            goalsStrings = typeof(GoapStates).GetEnumNames();

        foreach (string s in statesStrings)
        {
            if (s == null || s == "None") continue;
            states.Add(new WorldState() { key = s });
        }
        foreach (string s in goalsStrings)
        {
            if (s == null || s == "None") continue;
            states.Add(new WorldState() { key = s });
        }

        return states;
    }
    public static List<WorldState> GetAllStatesFromFlags(GoapGoals goalsEnum)
    {
        List<WorldState> states = new List<WorldState>();

        string[] goalsStrings = goalsEnum.ToString().Split(", ");
        if ((int)goalsEnum == -1)
            goalsStrings = typeof(GoapGoals).GetEnumNames();

        foreach (string s in goalsStrings)
        {
            if (s == null || s == "None") continue;
            states.Add(new WorldState() { key = s });
        }
        return states;
    }
    public static List<WorldState> GetAllStatesFromFlags(GoapStates statesEnum)
    {
        List<WorldState> states = new List<WorldState>();

        string[] enumStrings = statesEnum.ToString().Split(", ");
        if ((int)statesEnum == -1)
            enumStrings = typeof(GoapStates).GetEnumNames();

        foreach (string s in enumStrings)
        {
            if (s == null || s == "None") continue;
            states.Add(new WorldState() { key = s });
        }
        return states;
    }
}