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
        if (RewardsDisplayScript.IsRewarding || WinLossManager.Instance != null && WinLossManager.Instance.IsGameComplete) return;

        IsHovered = true;

        DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false); //sending this transform so all button will enable BCs

        foreach (var card in DeckAndHandManager.Instance.CardsInHand)
        {
            if (!card.GetCardTransform.TryGetComponent(out CardSelect cs) ||
                !card.GetCardTransform.TryGetComponent(out CardFunctionScript cfs)) continue;

            if (!cfs.IsSelected && !cfs.IsDragging && !PauseMenu.isPaused)
            {
                cs.ToggleHighlightAndScale(false);

                if (DeckAndHandManager.Instance != null && DeckAndHandManager.Instance.GetSelectedCard != null) return;

                APDisplay.Instance?.ClearPreview();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (RewardsDisplayScript.IsRewarding || WinLossManager.Instance != null && WinLossManager.Instance.IsGameComplete) return;

        IsHovered = false;
    }
}
