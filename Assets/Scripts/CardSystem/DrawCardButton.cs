using CardSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawCardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CardAbilityDefinition _drawAbility;
    private Unit _player;

    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_drawAbility == null) return;
        APDisplay.Instance.ShowPreview(_drawAbility.GetApCost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_drawAbility == null) return;
        APDisplay.Instance.ClearPreview();
    }
    private bool GrabPlayerUnit()
    {
        foreach (var u in TurnManager.GetUnitTurnOrder)
            if (u.GetTeam == Team.Friendly)
            {
                _player = u;
                return true;
            }
        return false;
    }
    public void DrawCardWithButton()
    {
        if (_player == null && !GrabPlayerUnit()) return;

        _drawAbility?.UseAbility(_player);
    }
}
