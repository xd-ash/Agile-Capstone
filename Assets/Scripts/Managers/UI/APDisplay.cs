using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class APDisplay : MonoBehaviour
{
    [Header("AP Box Display")]
    [SerializeField] private Transform _apBoxContainer;
    [SerializeField] private Transform _movementBoxContainer;
    [SerializeField] private GameObject _apBoxPrefab;
    [SerializeField] private Image[] _apFills;
    [SerializeField] private Image[] _movementFills;
    private int _lastMaxAP = -1;
    private int _lastMaxMovement = -1;

    private Unit _currentUnit;

    public static APDisplay Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnStart += OnTurnStart;
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnStart -= OnTurnStart;

        if (_currentUnit == null) return;

        _currentUnit.OnApChanged -= (x) => UpdateBoxes(x);
        _currentUnit.OnMovementChanged -= (x) => UpdateBoxes(x, false);
    }

    private void OnTurnStart(Unit unit)
    {
        if (unit == null) return;

        if (_currentUnit != null)
        {
            _currentUnit.OnApChanged -= (x) => UpdateBoxes(x);
            _currentUnit.OnMovementChanged -= (x) => UpdateBoxes(x, false);
        }

        _currentUnit = unit;

        if (unit.GetMaxAP != _lastMaxAP)
            BuildBoxes(unit.GetMaxAP);
        if (unit.GetMaxMovement != _lastMaxMovement)
            BuildBoxes(unit.GetMaxAP, false);

        _currentUnit.OnApChanged += (x) => UpdateBoxes(x);
        _currentUnit.OnMovementChanged += (x) => UpdateBoxes(x, false);

        UpdateBoxes(unit);
    }

    private void BuildBoxes(int max, bool isAP = true)
    {
        var boxContainer = isAP ? _apBoxContainer : _movementBoxContainer;

        foreach (Transform child in boxContainer)
            Destroy(child.gameObject);

        if (isAP)
        {
            ActuallyBuildBoxes(max, boxContainer, ref _apFills);
            _lastMaxAP = max;
        }
        else
        {
            ActuallyBuildBoxes(max, boxContainer, ref _movementFills);
            _lastMaxMovement = max;
        }
    }
    private void ActuallyBuildBoxes(int max, Transform parent, ref Image[] fills)
    {
        fills = new Image[max];

        for (int i = 0; i < max; i++)
        {
            GameObject box = Instantiate(_apBoxPrefab, parent);
            Transform fill = box.transform.Find("APFill");

            if (fill == null)
            {
                Debug.LogError($"[APDisplay] APBox prefab is missing a child named '{"APFill"}'. Check your prefab setup.");
                return;
            }

            fills[i] = fill.GetComponent<Image>();
        }
    }
    private void UpdateBoxes(Unit unit, bool isAP = true)
    {
        var fills = isAP ? _apFills : _movementFills;

        if (fills == null || unit == null) return;

        int currentAmount = isAP ? unit.GetAP : unit.GetMovementPoints;

        for (int i = 0; i < fills.Length; i++)
        {
            if (fills[i] == null) continue;
            fills[i].enabled = i < currentAmount;
        }
    }

    public void ShowPreview(int cost, bool isAP = true)
    {
        var fills = isAP ? _apFills : _movementFills;

        if (fills == null || _currentUnit == null) return;

        // Walk backwards through filled boxes and dim the ones that would be spent
        int previewCount = 0;
        for (int i = fills.Length - 1; i >= 0 && previewCount < cost; i--)
        {
            if (fills[i] == null || !fills[i].enabled) continue;

            fills[i].color = new Color(1f, 0.3f, 0.3f, 0.8f); // dim red tint
            previewCount++;
        }
    }

    public void ClearPreview(bool isAP = true)
    {
        if (isAP)
        {
            if (_apFills == null) return;

            foreach (var fill in _apFills)
                if (fill != null)
                    fill.color = Color.white;
        }
        else
        {
            if (_movementFills == null) return;

            foreach (var fill in _movementFills)
                if (fill != null)
                    fill.color = Color.white;
        }
    }

    /*public bool CanAfford(int cost)
    {
        return _currentUnit != null && _currentUnit.GetAP >= cost;
    }*/
}