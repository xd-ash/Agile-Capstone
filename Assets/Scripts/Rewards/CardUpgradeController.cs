using CardSystem;
using System;
using UnityEngine;

public class CardUpgradeController : MonoBehaviour
{
    private DeckViewerScript _deckViewPanel;
    [SerializeField] private GameObject _upgradePreviewPanel;
    [SerializeField] private Transform _cardPrefabParent;

    [Space(10), SerializeField] private Card _selectedCard;

    public static bool IsPreviewingUpgrade { get; private set; } = false;

    private Action _onComplete;

    public static CardUpgradeController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    public void InitUpgradeController(Action onComplete)
    {
        _onComplete = onComplete;

        _deckViewPanel = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);
        _upgradePreviewPanel = _deckViewPanel.transform.Find("CardUpgradePreview").gameObject;
        _cardPrefabParent = _upgradePreviewPanel.transform.Find("CardParent");
    }

    public void ShowUpgradePreview(Card cardToUpgrade)
    {
        _selectedCard = cardToUpgrade;

        if (cardToUpgrade.GetCardRarity == CardRarity.Epic)
        {
            Debug.LogWarning($"{cardToUpgrade.GetCardName} is max rarity (epic). Upgrade preview failed.");
            CloseUpgradePreview();
            return;
        }

        _upgradePreviewPanel?.SetActive(true);

        GameObject cardPrefab = Resources.Load<GameObject>("NewCardPrefab");

        CardRarity upgradedRarity = cardToUpgrade.GetNextCardRarity;

        GameObject initCard = Instantiate(cardPrefab, _cardPrefabParent);
        Card tempCard = new(cardToUpgrade,initCard.transform);
        CardPrefabSetterUpper.SetupCardPrefab(tempCard, CardState.UpgradeMenu);

        GameObject tempUpgradeCard = Instantiate(cardPrefab, _cardPrefabParent);
        Card tempUpgrade = new(cardToUpgrade.GetCardAbility, upgradedRarity, tempUpgradeCard.transform);
        CardPrefabSetterUpper.SetupCardPrefab(tempUpgrade, CardState.UpgradeMenu);

        IsPreviewingUpgrade = true;
    }
    public void UpgradeSelectedCard()
    {
        if (_selectedCard == null)
        {
            Debug.LogWarning("Selected card was null on upgrade attempt");
            return;
        }
        _selectedCard.UpgradeCard();
        CloseUpgradePreview();

        var deckViewWindow = DeckViewerScript.Instance;
        if (deckViewWindow == null)
        {
            Debug.LogError("DeckViewerScript instance is null.");
            return;
        }

        deckViewWindow?.gameObject?.SetActive(false);
        _onComplete?.Invoke();
    }
    public void CloseUpgradePreview()
    {
        ClearSelection(_selectedCard.GetCardTransform);
        _selectedCard = null;
        for (int i = _cardPrefabParent.childCount - 1; i >= 0; i--)
            Destroy(_cardPrefabParent.GetChild(i).gameObject);
        _upgradePreviewPanel.SetActive(false);
        IsPreviewingUpgrade = false;
    }
    private void ClearSelection(Transform cardTransform)
    {
        if (cardTransform == null) return;
        var cfs = cardTransform.GetComponentInParent<CardFunctionScript>();
        var cs = cardTransform.GetComponentInParent<CardSelect>();
        cs?.ToggleHighlightAndScale(false);
        cfs?.ClearSelection(0f);
    }
}