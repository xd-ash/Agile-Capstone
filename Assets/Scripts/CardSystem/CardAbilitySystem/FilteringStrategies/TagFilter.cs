using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    [CreateNodeMenu("Filter/Tag")]
    public class TagFilter : FilterStrategy
    {
        public string tagToCompare;

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> objectsToFilter)
        {
            foreach (var obj in objectsToFilter)
            {
                if (obj.CompareTag(tagToCompare))
                {
                    yield return obj;
                }
            }
        }
    }
}