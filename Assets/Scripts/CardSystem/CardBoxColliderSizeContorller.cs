using CardSystem;
using UnityEngine;

public class CardBoxColliderSizeContorller : MonoBehaviour
{
    [Header("Card Hand BC Lerp")]
    // vector2s representing min & max offset x and min & max size x **Vector2(min x, max x)
    [SerializeField] private Vector2 _offsetMinMaxX;
    [SerializeField] private Vector2 _sizeMinMaxX;

    [Header("Combat Entensions")]
    [SerializeField] private Vector2 _sizeMinMaxY;

    private BoxCollider2D _mainBC;

    private void Awake()
    {
        _mainBC = GetComponents<BoxCollider2D>()?[0];

        if (DeckAndHandManager.Instance == null) return;
        DeckAndHandManager.OnUpdateCardColliders += LerpBCSizeAndOffset;
    }

    private void OnDestroy()
    {
        if (DeckAndHandManager.Instance == null) return;
        DeckAndHandManager.OnUpdateCardColliders -= LerpBCSizeAndOffset;
    }

    public void LerpBCSizeAndOffset(Transform t) => LerpBCSizeAndOffset(t == transform);
    public void LerpBCSizeAndOffset(bool isHovered)
    {
        var dhm = DeckAndHandManager.Instance;
        if (dhm == null || _mainBC == null) return;
        var cardHandRatio = (float)dhm.CardsInHand.Count / (float)dhm.GetMaxHandSize;
        var thisSiblingIndex = transform.GetSiblingIndex();

        bool isLastCardChild = thisSiblingIndex >= dhm.CardsInHand.Count - 1;

        var maxOffset = new Vector2(_offsetMinMaxX.y, _mainBC.offset.y);
        var minOffset = new Vector2(_offsetMinMaxX.x, _mainBC.offset.y);
        var maxSize = new Vector2(_sizeMinMaxX.y, _mainBC.size.y);
        var minSize = new Vector2(_sizeMinMaxX.x, _mainBC.size.y);

        _mainBC.offset = isLastCardChild || isHovered ? maxOffset : Vector2.Lerp(maxOffset, minOffset, cardHandRatio);
        _mainBC.size = isLastCardChild || isHovered ? maxSize : Vector2.Lerp(maxSize, minSize, cardHandRatio);
    }
    public void ExtendBCForCombat(bool isExtended)
    {
        var maxSize = new Vector2(_mainBC.size.x, _sizeMinMaxY.y);
        var minSize = new Vector2(_mainBC.size.x, _sizeMinMaxY.x);

        _mainBC.size = isExtended ? maxSize : minSize;
    }
}
