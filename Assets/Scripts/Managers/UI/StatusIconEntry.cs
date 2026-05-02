using TMPro;
using UnityEngine;
using UnityEngine.UI;

// [STATUS_UI] Single status effect icon entry in the UI
public class StatusIconEntry : MonoBehaviour
{
    [SerializeField] private Image _effectIcon;        // [STATUS_UI] the effect's unique icon
    [SerializeField] private Image _clockIcon;         // [STATUS_UI] clock sprite
    [SerializeField] private TextMeshProUGUI _turnText; // [STATUS_UI] remaining turn count

    public void Setup(Sprite effectSprite, int turnsRemaining)
    {
        if (_effectIcon != null)
            _effectIcon.sprite = effectSprite;

        UpdateTurns(turnsRemaining);
    }

    public void UpdateTurns(int turnsRemaining)
    {
        if (_turnText != null)
            _turnText.text = turnsRemaining.ToString();
    }
}