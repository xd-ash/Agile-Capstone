using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace CardSystem
{
    public abstract class FilterStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public int enter;

        public abstract IEnumerable<GameObject> Filter(IEnumerable<GameObject> objectsToFilter);
    }
}