using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    //Concrete filter strategy to filter based on gameobject layer
    [CreateNodeMenu("Filter/Layer Filter")]
    public class LayerFilter : FilterStrategy
    {
        [SerializeField] private LayerMask _layermask;

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> objectsToFilter)
        {
            foreach (var obj in objectsToFilter)
                if (obj != null && (_layermask & (1 << obj.layer)) != 0) // Bitwise operations, not sure if done correctly
                    yield return obj;
        }
    }
}