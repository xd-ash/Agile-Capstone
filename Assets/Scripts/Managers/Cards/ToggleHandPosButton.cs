using CardSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleHandPosButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsHovered { get; private set; } = false;

    public static ToggleHandPosButton Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;

        foreach (var card in DeckAndHandManager.Instance.CardsInHand)
        {
            if (!card.GetCardTransform.TryGetComponent(out CardSelect cs) ||
                !card.GetCardTransform.TryGetComponent(out CardFunctionScript cfs)) continue;

            if (!cfs.IsSelected && !cfs.IsDragging && !PauseMenu.isPaused)
            {
                cs.ToggleHighlightAndScale(false);
                DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);

                if (DeckAndHandManager.Instance != null && DeckAndHandManager.Instance.GetSelectedCard != null) return;

                APDisplay.Instance?.ClearPreview();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
    }
}
