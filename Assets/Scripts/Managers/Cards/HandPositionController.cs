using CardSystem;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class HandPositionController : MonoBehaviour
{
    private SplineContainer _splineContainer;
    [SerializeField, Space(10)] private Vector3 _splineKnotRatios;

    [SerializeField] private Transform _handUpPos, _handDownPos;
    [SerializeField] private Button _toggleButton;
    private GameObject _arrowDown, _arrowUp;

    [SerializeField] private float _handMoveSpeed = 1f;
    [SerializeField] private float _onStartHandLowerDelay = 1.5f;

    private bool _isHandUp = true;

    public bool IsHandUp => _isHandUp;
    public float GetCardActivePosYAdjustment => -(transform.localPosition.y + (_isHandUp ? _handUpPos.position.y : _handDownPos.position.y));

    public static HandPositionController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _arrowDown = _toggleButton.transform.GetChild(0).gameObject;
        _arrowUp = _toggleButton.transform.GetChild(1).gameObject;

        _splineContainer = FindFirstObjectByType<SplineContainer>();
    }
    private void Start()
    {
        // toggle to lower position to start
        StartCoroutine(DelayedStartHandLower());
    }

    private IEnumerator DelayedStartHandLower()
    {
        yield return new WaitForSeconds(_onStartHandLowerDelay);
        ToggleHandPosition();
    }
    public void ToggleHandPosition()
    {
        _isHandUp = !_isHandUp;

        var tarPos = _isHandUp ? _handUpPos : _handDownPos;
        var selectedCard = DeckAndHandManager.Instance.GetSelectedCard;
        Transform cardTrans = selectedCard?.GetCardTransform;

        _arrowUp.SetActive(!_isHandUp);
        _arrowDown.SetActive(_isHandUp);
        SetSplineKnotValues();

        StopAllCoroutines();
        StartCoroutine(HandMoveCoro(tarPos, cardTrans));
    }
    private void SetSplineKnotValues()
    {
        var bezierKnot0 = _splineContainer.Spline.Knots.ElementAt(0);
        var bezierKnot1 = _splineContainer.Spline.Knots.ElementAt(1);
        var bezierKnot2 = _splineContainer.Spline.Knots.ElementAt(2);
        bezierKnot0.Position = _splineKnotRatios.x * (_isHandUp ? _handUpPos.position.y : _handDownPos.position.y);
        bezierKnot1.Position = _splineKnotRatios.y * (_isHandUp ? _handUpPos.position.y : _handDownPos.position.y);
        bezierKnot2.Position = _splineKnotRatios.z * (_isHandUp ? _handUpPos.position.y : _handDownPos.position.y);
    }
    private IEnumerator HandMoveCoro(Transform tarPos, Transform selectedCardTransform)
    {
        float dist = Mathf.Abs(transform.localPosition.y - tarPos.localPosition.y);

        while (transform.localPosition != tarPos.localPosition)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, tarPos.localPosition, _handMoveSpeed);
            if (selectedCardTransform != null)
                selectedCardTransform.localPosition = Vector3.MoveTowards(selectedCardTransform.localPosition, 
                    selectedCardTransform.localPosition + Vector3.up * (_isHandUp ? -dist : dist) , _handMoveSpeed);
            yield return null;
        }
    }
}
