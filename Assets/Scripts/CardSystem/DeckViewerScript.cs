using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewerScript : MonoBehaviour
{
    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _deckScrollView;
    [SerializeField] private TextMeshProUGUI _deckTitleText;

    public static DeckViewerScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        BuildDeckScrollViewContent();
    }

    //Create all card content in the card library scrollview
    private void BuildDeckScrollViewContent()
    {
        if (PlayerDataManager.Instance == null || _cardContentPrefab == null || _deckScrollView == null) return;
        if (PlayerDataManager.Instance.GetActiveDeck == null || PlayerDataManager.Instance.GetActiveDeck.GetCardsInDeck == null)
        {
            Debug.Log("Playerdata deck error");
            return;
        }

        var deck = PlayerDataManager.Instance.GetActiveDeck;
        if (_deckTitleText != null)
            _deckTitleText.text = $"All Cards in {deck.GetDeckName}";

        ClearScrollviewContent(_deckScrollView.content);

        int contentIndex = -1;
        foreach (var card in deck.GetCardsInDeck)
        {
            if (card == null) continue;

            GameObject content = Instantiate(_cardContentPrefab, Vector3.zero, Quaternion.identity, _deckScrollView.content);

            TextMeshProUGUI[] cardTextFieldsUI = content.GetComponentsInChildren<TextMeshProUGUI>();
            // Update text content
            cardTextFieldsUI[0].text = card.GetCardName;
            cardTextFieldsUI[1].text = card.GetDescription;
            cardTextFieldsUI[2].text = card.GetApCost.ToString();

            contentIndex++;
            var rt = content.GetComponent<RectTransform>();
            //SetCardLibraryContentTransform(ref rt, contentIndex);

            var addCardButton = content.GetComponentInChildren<Button>();
            if (addCardButton == null) continue;
            addCardButton?.gameObject.SetActive(false);
        }
    }
    private void ClearScrollviewContent(RectTransform contentTransform)
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            Destroy(contentTransform.GetChild(i).gameObject);
    }
}
