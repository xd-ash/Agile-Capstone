using UnityEngine;
using UnityEngine.UI;

namespace CardSystem
{
    public class DeckViewerCardVisuals : MonoBehaviour
    {
        [SerializeField] private Image _categoryBorder;
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _categoryIcon;

        public void ApplyVisuals(CardAbilityDefinition def)
        {
            if (def == null) return;

            CardVisualConfig config = Resources.Load<CardVisualConfig>("CardVisualConfig");
            if (config == null)
            {
                Debug.LogError("[DeckViewerCardVisuals] CardVisualConfig not found in Resources.");
                return;
            }

            if (!config.TryGetVisual(def.GetCardCategory, out var visual)) return;

            if (_categoryBorder != null)
                _categoryBorder.color = visual.borderColor;

            if (_categoryIcon != null)
            {
                _categoryIcon.sprite = visual.icon;
                _categoryIcon.gameObject.SetActive(visual.icon != null);
            }

            if (_cardImage != null)
            {
                _cardImage.sprite = def.GetCardImage;
                _cardImage.gameObject.SetActive(def.GetCardImage != null);
            }
        }
    }
}