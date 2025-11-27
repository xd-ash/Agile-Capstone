using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardSystem
{
    // Attach to UI prefab root. Prefab must contain Button and the TMP text fields wired below.
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI apText;

        private Card _card;

        public void Bind(Card card)
        {
            _card = card;
            if (_card == null) return;

            nameText.text = _card.GetCardName ?? "Unnamed";
            descText.text = _card.GetDescription ?? "";
            apText.text = _card.GetCardAbility?.RootNode?.GetApCost.ToString() ?? "-";

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            if (PauseMenu.isPaused) return;
            if (_card == null) return;

            // Tell CardManager to select/use this card (centralized behavior)
            CardManager.instance?.SelectCard(_card);
        }
    }
}