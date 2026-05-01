using UnityEngine;
using UnityEngine.UI;

public class CardRemovalButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        ToggleInteractable();
    }
    public void ToggleInteractable()
    {
        if (_button == null || PlayerDataManager.Instance == null) return;
        _button.interactable = PlayerDataManager.Instance.GetBalance >= CardShopManager.Instance.GetRemovalCost;
    }
    public void OnClick()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.GetBalance < CardShopManager.Instance.GetRemovalCost || CurrencyManager.Instance == null) return;

        var deckViewer = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);
        deckViewer?.gameObject.SetActive(true);
        deckViewer.InitDeckViewer((x) =>
        {
            CurrencyManager.Instance.TrySpend(CardShopManager.Instance.GetRemovalCost);
            ToggleInteractable();
        }, CardState.CardRemoval);
    }
}
