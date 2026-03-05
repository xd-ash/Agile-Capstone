using CardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

            TextMeshProUGUI[] cardTextFieldsUI = content.GetComponentsInChildren<TextMeshProUGUI>();
            // Update text content
            cardTextFieldsUI[0].text = cardAbility.GetCardName;
            cardTextFieldsUI[1].text = cardAbility.GetDescription;
            cardTextFieldsUI[2].text = cardAbility.GetApCost.ToString();

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
