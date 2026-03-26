using UnityEngine;

public class TileMapObjRepositioner : MonoBehaviour
{
    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 
                child.localPosition.y * IsoMetricConversions.SpriteAdjustmentZDir);
        }
    }
}
