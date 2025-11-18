using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum GoapActions
{
    None = 0,
    //All = -1,
    Move = 2,
    Attack = 4,
    Heal = 8
}

[Flags]
public enum GoapStates
{
    None = 0,
    //All = -1,
    HasAP = 2,
    IsHurt = 4,
    InRange = 8,
}

public struct GOAPEnums
{
    // Create and return a list of all independent DamageTypes of a given enum flag.
    public static List<GoapAction> GetAllTypesFromFlags(GoapActions actionsEnum)
    {
        List<GoapAction> actions = new List<GoapAction>();

        // Convert dmgType enum flag to binary.
        string binaryEnum = Convert.ToString((int)actionsEnum, 2).PadLeft(8, '0');

        // Loop through each character in the binaryEnum string and add relevant
        // DamageTypes to the list.
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
                case 3://not implemented
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
                        actions.Add(new MoveAction());
                    break;
                case 7://None
                    break;
            }
        }
        return actions;
    }
}