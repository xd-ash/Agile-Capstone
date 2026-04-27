using UnityEngine;

public class ReturnToMapButton : MonoBehaviour
{
    public void OnClickReturn()
    {
        if (ShopConfirmPopup.Instance != null && ShopConfirmPopup.Instance.gameObject.activeInHierarchy) return;

        NodeMapManager.Instance.CompleteCurrentNode();
        NodeMapManager.Instance.ReturnToMap();
    }
}