using UnityEngine;

// [PAUSE_OVERLAY] Attach to a canvas or manager that stays active.
// Toggles a referenced child GameObject off when pause/settings opens, back on when closed.
public class PauseOverlayHider : MonoBehaviour
{
    [SerializeField] private GameObject _hudGroup;

    private void OnEnable()
    {
        PauseMenu.OnMenuOverlayChanged += OnOverlayChanged;
    }

    private void OnDisable()
    {
        PauseMenu.OnMenuOverlayChanged -= OnOverlayChanged;
    }

    private void OnOverlayChanged(bool overlayActive)
    {
        if (_hudGroup != null)
            _hudGroup.SetActive(!overlayActive);
    }
}