using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitActionSO", menuName = "New Unit Action SO")]
public class UnitActionSOBase : ScriptableObject
{
    public bool action1;
    public bool action2;
    public bool action3;

    public List<ActionBase> GetActions()
    {
        var actionlist = new List<ActionBase>();

        if (action1)
            actionlist.Add(new TestAction());
        if (action2)
            actionlist.Add(new Test2Action());
        if (action3)
            actionlist.Add(new Test3Action());

        return actionlist;
    }
}
