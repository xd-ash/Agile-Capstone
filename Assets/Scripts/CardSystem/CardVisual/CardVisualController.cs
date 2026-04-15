using UnityEngine;

namespace CardSystem
{
    public class CardVisualController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _cardImage;
        [SerializeField] private SpriteRenderer _categoryBorder;
        [SerializeField] private SpriteRenderer _categoryIcon;
        [SerializeField] private SpriteRenderer _apSymbol;

        private static CardVisualConfig _config;

        private static CardVisualConfig Config
        {
            get
            {
                if (_config == null)
                    _config = Resources.Load<CardVisualConfig>("CardVisualConfig");

                if (_config == null)
                    Debug.LogError("[CardVisualController] CardVisualConfig not found in Resources folder.");

                return _config;
            }
        }

        public void ApplyVisuals(CardAbilityDefinition def)
        {
            if (def == null || Config == null) return;

            if (!Config.TryGetVisual(def.GetCardCategory, out var visual)) return;

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
        
        public void UpdateSortingOrder(int sortingOrder)
        {
            if (_categoryBorder != null)
                _categoryBorder.sortingOrder = sortingOrder + 2;

            if (_apSymbol != null)
                _apSymbol.sortingOrder = sortingOrder + 2;

            if (_categoryIcon != null)
                _categoryIcon.sortingOrder = sortingOrder + 3; // above card body and text
            
            if (_cardImage != null)
                _cardImage.sortingOrder = sortingOrder + 1;
        }

        public void FillTextFields()
        {

        }
    }
}