using UnityEngine;

public class ReturnToMapButton : MonoBehaviour
{
    public void OnClickReturn()
    {
        NodeMapManager.Instance.CompleteCurrentNode();
        NodeMapManager.Instance.ReturnToMap();
    }
}