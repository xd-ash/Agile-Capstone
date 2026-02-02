using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardSystem; // Make sure this matches your namespace

public class DeckPreviewRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button inspectButton;

    private CardAbilityDefinition _cardDef;

    /// <summary>
    /// Assigns the card definition and how many of this card are in the deck
    /// </summary>
    public void Bind(CardAbilityDefinition def, int count)
    {
        if (def == null)
        {
            Debug.LogError("DeckPreviewRow.Bind called with null def!");
            return;
        }

        _cardDef = def;

        if (nameText != null)
            nameText.text = def.GetCardName;

        if (countText != null)
            countText.text = count.ToString();

        if (inspectButton != null)
        {
            inspectButton.onClick.RemoveAllListeners();
            inspectButton.onClick.AddListener(OnClick);
        }
    }

    /// <summary>
    /// Called when player clicks this row — opens inspect popup
    /// </summary>
    public void OnClick()
    {
        if (_cardDef == null)
        {
            Debug.LogWarning("DeckPreviewRow.OnClick: _cardDef is null");
            return;
        }

        // Open the card inspect popup
        CardInspectPopup.Instance.Show(_cardDef);
    }
}
