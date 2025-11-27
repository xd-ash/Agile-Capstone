using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public class HandUIManager : MonoBehaviour
    {
        public static HandUIManager instance;

        [Header("UI refs")]
        [SerializeField] private RectTransform handContainer;   // panel with a HorizontalLayoutGroup
        [SerializeField] private GameObject cardUIPrefab;       // prefab containing CardUI

        private readonly Dictionary<Card, CardUI> _map = new();

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        public void AddCardUI(Card card)
        {
            if (card == null || cardUIPrefab == null || handContainer == null) return;

            GameObject go = Instantiate(cardUIPrefab, handContainer);
            var ui = go.GetComponent<CardUI>();
            if (ui == null)
            {
                Debug.LogWarning("[HandUIManager] cardUIPrefab is missing CardUI component.");
                Destroy(go);
                return;
            }

            ui.Bind(card);
            _map[card] = ui;
        }

        public void RemoveCardUI(Card card)
        {
            if (card == null) return;
            if (_map.TryGetValue(card, out var ui))
            {
                if (ui != null) Destroy(ui.gameObject);
                _map.Remove(card);
            }
        }

        public void ClearAll()
        {
            foreach (var kv in _map)
                if (kv.Value != null) Destroy(kv.Value.gameObject);
            _map.Clear();
        }
    }
}