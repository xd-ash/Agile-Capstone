using System;
using System.Collections.Generic;

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
    OtherMove = 128,
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
}
[Flags]
public enum GoapGoals
{
    None = 0,
    KillPlayer = 2,
    //hurt player? more calcs for ability dmg, player health, heuristics, etc
    StayAlive = 4,
    KeepAlliesAlive = 8,
    EndTurn = 16
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
                case 0://Other Move
                    if (binaryEnum[i] == '1')
                        actions.Add(new OtherMoveAction());
                    break;
                case 1://MoveIntoLOS
                    if (binaryEnum[i] == '1')
                        actions.Add(new MoveIntoLOSAction());
                    break;
                case 2://EndTurn
                    if (binaryEnum[i] == '1')
                        actions.Add(new EndTurnAction());
                    break;
                case 3://Choose Target
                    if (binaryEnum[i] == '1')
                        actions.Add(new ChooseTargetAction());
                    break;
                case 4://Heal
                    if (binaryEnum[i] == '1')
                        actions.Add(new HealAction());
                    break;
                case 5://Attack
                    if (binaryEnum[i] == '1')
                        actions.Add(new AttackAction());
                    break;
                case 6://Move
                    if (binaryEnum[i] == '1')
                        actions.Add(new MoveInRangeAction());
                    break;
                case 7://None
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

        foreach (string s in enumStrings)
        {
            if (s == null || s == "None") continue;
            states.Add(new WorldState() { key = s });
        }
        return states;
    }
}