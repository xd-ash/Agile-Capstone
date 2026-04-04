using CardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewerScript : MonoBehaviour
{
    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _deckScrollView;

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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameObject.SetActive(false);
    }
    //Create all card content in the card library scrollview
    private void BuildDeckScrollViewContent()
    {
        if (PlayerDataManager.Instance == null || _cardContentPrefab == null || _deckScrollView == null) return;
        if (PlayerDataManager.Instance.GetPlayerDeck == null || PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck == null)
        {
            Debug.Log("Playerdata deck error");
            return;
        }

        var deck = PlayerDataManager.Instance.GetPlayerDeck;       
        if (deck == null) return;

        ClearScrollviewContent(_deckScrollView.content);

        foreach (var card in deck.GetCardsInDeck)
        {
            if (card == null) continue;
            var cardAbility = card.GetCardAbility;

            GameObject content = Instantiate(_cardContentPrefab, Vector3.zero, Quaternion.identity, _deckScrollView.content);
            Card tempCard = new(card, content.transform); 
            CardPrefabSetterUpper.SetupCardPrefab(tempCard, CardState.DeckViewer);

            //var addCardButton = content.GetComponentInChildren<Button>();
            //if (addCardButton == null) continue;
            //addCardButton?.gameObject.SetActive(false);
        }
    }
    private void ClearScrollviewContent(RectTransform contentTransform)
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            Destroy(contentTransform.GetChild(i).gameObject);
    }
}
