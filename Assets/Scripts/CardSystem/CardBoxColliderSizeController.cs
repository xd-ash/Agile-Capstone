using CardSystem;
using UnityEngine;

public class CardBoxColliderSizeController : MonoBehaviour
{
    [Header("Card Hand BC Lerp")]
    // vector2s representing min & max offset x and min & max size x **Vector2(min x, max x)
    [SerializeField] private Vector2 _offsetMinMaxX;
    [SerializeField] private Vector2 _sizeMinMaxX;

    [Header("Combat Entensions")]
    [SerializeField] private Vector2 _offsetAndSizeMaxY;

    private BoxCollider2D _bc;

    private void Awake()
    {
        _bc = GetComponent<BoxCollider2D>();

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
        if (dhm == null || _bc == null) return;
        var cardHandRatio = (float)dhm.CardsInHand.Count / (float)dhm.GetMaxHandSize;
        var thisSiblingIndex = transform.GetSiblingIndex();

        bool isLastCardChild = thisSiblingIndex >= dhm.CardsInHand.Count - 1;

        var maxOffset = new Vector2(_offsetMinMaxX.y, _bc.offset.y);
        var minOffset = new Vector2(_offsetMinMaxX.x, _bc.offset.y);
        var maxSize = new Vector2(_sizeMinMaxX.y, _bc.size.y);
        var minSize = new Vector2(_sizeMinMaxX.x, _bc.size.y);

        _bc.offset = isLastCardChild || isHovered ? maxOffset : Vector2.Lerp(maxOffset, minOffset, cardHandRatio);
        _bc.size = isLastCardChild || isHovered ? maxSize : Vector2.Lerp(maxSize, minSize, cardHandRatio);
    }
    public void ExtendBCForCombat()
    {
        if (_bc == null) return;
        _bc.offset = new Vector2(_bc.offset.x, _offsetAndSizeMaxY.x);
        _bc.size = new Vector2(_bc.size.x, _offsetAndSizeMaxY.y);
    }
}
