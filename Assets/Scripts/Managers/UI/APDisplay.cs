using UnityEngine;
using UnityEngine.UI;

public class APDisplay : MonoBehaviour
{
    [Header("AP Box Display")]
    [SerializeField] private Transform _boxContainer;
    [SerializeField] private GameObject _apBoxPrefab;
    private Image[] _apFills;
    private Unit _currentUnit;
    private int _lastMaxAP = -1;
    
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

        if (_currentUnit != null)
            _currentUnit.OnApChanged -= UpdateBoxes;
    }
    
    private void OnTurnStart(Unit unit)
    {
        if (unit == null) return;

        if (_currentUnit != null)
            _currentUnit.OnApChanged -= UpdateBoxes;

        _currentUnit = unit;

        if (unit.GetMaxAP != _lastMaxAP)
            BuildBoxes(unit.GetMaxAP);

        _currentUnit.OnApChanged += UpdateBoxes;
        
        UpdateBoxes(unit);
    }
    
    private void BuildBoxes(int maxAP)
    {
        foreach (Transform child in _boxContainer)
            Destroy(child.gameObject);

        _apFills = new Image[maxAP];

        for (int i = 0; i < maxAP; i++)
        {
            GameObject box = Instantiate(_apBoxPrefab, _boxContainer);
            Transform fill = box.transform.Find("APFill");

            if (fill == null)
            {
                Debug.LogError("[APDisplay] APBox prefab is missing a child named 'APFill'. Check your prefab setup.");
                return;
            }

            _apFills[i] = fill.GetComponent<Image>();
        }

        _lastMaxAP = maxAP;
    }
    private void UpdateBoxes(Unit unit)
    {
        if (_apFills == null || unit == null) return;

        int currentAP = unit.GetAP;

        for (int i = 0; i < _apFills.Length; i++)
        {
            if (_apFills[i] != null)
                _apFills[i].enabled = i < currentAP;
        }
    }
    
    public void ShowPreview(int cost)
    {
        if (_apFills == null || _currentUnit == null) return;

        int currentAP = _currentUnit.GetAP;

        // Walk backwards through filled boxes and dim the ones that would be spent
        int previewCount = 0;
        for (int i = _apFills.Length - 1; i >= 0 && previewCount < cost; i--)
        {
            if (_apFills[i] == null || !_apFills[i].enabled) continue;

            _apFills[i].color = new Color(1f, 0.3f, 0.3f, 0.8f); // dim red tint
            previewCount++;
        }
    }

    public void ClearPreview()
    {
        if (_apFills == null) return;

        foreach (var fill in _apFills)
            if (fill != null)
                fill.color = Color.white;
    }

    public bool CanAfford(int cost)
    {
        return _currentUnit != null && _currentUnit.GetAP >= cost;
    }
}