using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    //Concrete filter strategy to filter based on gameobject tag
    [CreateNodeMenu("Filter/Tag Filter")]
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