using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Base abstract Filter Strategy
    public abstract class FilterStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public double input; 

        public abstract IEnumerable<GameObject> Filter(IEnumerable<GameObject> objectsToFilter, Unit unit);
    }
}