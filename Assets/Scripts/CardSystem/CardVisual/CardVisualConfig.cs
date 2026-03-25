using UnityEngine;

namespace CardSystem
{
    [CreateAssetMenu(fileName = "CardVisualConfig", menuName = "Card System/Card Visual Config")]
    public class CardVisualConfig : ScriptableObject
    {
        [System.Serializable]
        public class CategoryVisual
        {
            public CardCategory category;
            public Color borderColor = Color.white;
            public Sprite icon;
        }

        [SerializeField] private CategoryVisual[] _categoryVisuals;

        public bool TryGetVisual(CardCategory category, out CategoryVisual result)
        {
            foreach (var visual in _categoryVisuals)
            {
                if (visual.category == category)
                {
                    result = visual;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}