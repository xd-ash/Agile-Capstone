using System;
using UnityEngine;

/* Potential for system that grabs from SO or other script
 * to avoid goap actions hvaing to be attached directly onto agent transform
[Flags]
public enum GoapActions
{
    None = 0,
    All = -1,
    Move = 2,
    Attack = 4,
    Heal = 8
}
*/
[Flags]
public enum GoapStates
{
    None = 0,
    All = -1,
    HasAP = 2,
    IsHurt = 4,
    InRange = 8,
}
